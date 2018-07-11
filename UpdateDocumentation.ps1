$ErrorActionPreference = 'Stop'

try {
  Get-Command pandoc | Out-Null
} catch {
  Write-Warning "Please install Pandoc - choco install pandoc --version 1.17.0.2 -y"
  exit 1
}

if ($null -eq (Get-ChildItem -Path 'choco.wiki' -File)) {
    git submodule init
}

git submodule update --remote --rebase

function CleanUpHeaderIds($text) {
  $text | % {
    [Regex]::Replace($_, '<h\d id="[^"]+', { $args[0] -replace '\.', '' })
  }
}

# https://github.com/jgm/pandoc/issues/2923
function Convert-MarkdownLinks($text) {

  $text = $text -replace '\[\[([^\|\]]*)\|([^\|\]]*)\]\]', '<a href="$2">$1</a>'
  #$text = $text -replace '\[\[([^\|\]]*)\]\]', ' <a href="$1">$1</a>'

  Write-Output $text
}

function Convert-SeoUrls($text) {

  Select-String '(href\=\"([^\:\#\"]+)(\#?[^:\"]*)\")' -input $text -AllMatches | % { $_.Matches } | % {
    # write-host $_.Value
    # Write-Host "0 - $($_.Groups[0])"
    # Write-Host "1 - $($_.Groups[1])"
    # Write-Host "url - $($_.Groups[2])"
    # Write-Host "hash - $($_.Groups[3])"

    $fullMatchValue = $_.Groups[1]
    $matchUrl = $_.Groups[2]
    $matchUrlReplace = $matchUrl -creplace '([A-Z])', '-$1'
    $matchUrlReplace = $matchUrlReplace.ToLower().Replace("--","-")
    $matchUrlReplace = $matchUrlReplace.Replace("-f-a-q","-faq")
    $matchUrlReplace = $matchUrlReplace.Replace("-p-s1","-ps1")

    $matchPound = $_.Groups[3]
    $matchPoundReplace = $matchPound -creplace '([A-Z])', '-$1'
    $matchPoundReplace = $matchPoundReplace.ToLower().Replace("--","-")

    if ($matchUrlReplace -ne $null -and $matchUrlReplace -ne '') {
      $text = $text.Replace($fullMatchValue, "href=`"@Url.RouteUrl(RouteName.Docs, new { docName = `"$matchUrlReplace`" })$matchPoundReplace`"")
    }
  }

  $text = $text.Replace("`"-","`"")

  Write-Output $text
}

function Fix-MarkdownConversion($text) {

  $text = $text -creplace 'â€”', '--'
  $text = $text -creplace 'â€“', '–'
  $text = $text -creplace 'â€¦', '...'
  $text = $text -creplace 'â€˜', '&#39;'
  $text = $text -creplace 'â€™', '&#39;'
  $text = $text -creplace 'â€œ', '&quot;'
  $text = $text -creplace 'â€', '&quot;'
  $text = $text -creplace 'Â', '&nbsp;'
  $text = $text.Replace("<!--remove","")
  $text = $text.Replace("remove-->","")

  Write-Output $text
}

function Convert-FencedCode($text) {

  #$text = $text.Replace("<pre><code>","<pre class=`"brush: plain`">")
  $text = $text.Replace("<pre><code>","<pre><code class=`"nohighlight`">")
  $text = $text -replace '\<pre\s?([^\>]*)\>\<code\>', '<pre><code $1>'
  #$text = $text.Replace("</code></pre>","</pre>")
  #$text = $text.Replace("class=`"ruby`"","class=`"brush: ruby`"")
  #$text = $text.Replace("class=`"puppet`"","class=`"brush: ruby`"")
  $text = $text.Replace("class=`"sh`"","class=`"nohighlight`"")
  #$text = $text.Replace("class=`"xml`"","class=`"brush: xml`"")
  #$text = $text.Replace("class=`"powershell`"","class=`"brush: ps`"")
  #$text = $text.Replace("class=`"yaml`"","class=`"brush: plain`"")
  #$text = $text.Replace("class=`"python`"","class=`"brush: python`"")
  #$text = $text.Replace("class=`"csharp`"","class=`"brush: csharp`"")

  Write-Output $text
}

function Convert-ImageUrls($text) {
  $text = $text -replace 'img\ssrc="images\/([^"]+)"', 'img src="@Url.Content("~/content/images/docs/$1")"'

  Write-Output $text
}

function Get-FirstLine($text) {
  $lineNumber = ($text | Select-String -pattern '@{' -SimpleMatch | Select -First 1).LineNumber

  Write-Output $lineNumber
}

Get-ChildItem -Path choco.wiki -Recurse -ErrorAction SilentlyContinue -Filter *.md | %{
  $docName = [System.IO.Path]::GetFileNameWithoutExtension($_)
  #$htmlFileName = "docgen\$docName.cshtml"
  $htmlFileName = "chocolatey\Website\Views\Documentation\$($docName.Replace(`"-`", `"`")).cshtml"

  #+simple_tables+native_spans+native_divs+multiline_tables
  # This is for pandoc 2.1.3 and above
  # & pandoc.exe --from gfm+old_dashes --tab-stop=2 --to html5 --no-highlight -V lang="en" -B docgen/header.txt -A docgen/footer.txt -o "$htmlFileName" "$($_.FullName)"
  & pandoc.exe --from markdown_github --normalize --tab-stop=2 --to html5 --old-dashes --no-highlight -V lang="en" -B docgen/header.txt -A docgen/footer.txt -o "$htmlFileName" "$($_.FullName)"

  $fileContent = Convert-SeoUrls (Convert-ImageUrls (Convert-FencedCode (Fix-MarkdownConversion (Convert-MarkdownLinks (CleanUpHeaderIds (Get-Content $htmlFileName).Replace("@","@@").Replace("{{AT}}","@").Replace("{{DocName}}",$docName))))))
  $firstLine = Get-FirstLine($fileContent)
  $firstLine -= 1
  Write-Debug "Line number is $firstLine"
  #[int]$firstLine = 13
  $fileContent |
    Select -Index ($firstLine..$($fileContent.count -3)) |
    Set-Content $htmlFileName -Force -Encoding UTF8
}

# copy the images
Copy-Item "choco.wiki\images\*" "chocolatey\Website\content\images\docs" -Force -Recurse
