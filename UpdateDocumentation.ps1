$ErrorActionPreference = 'Stop'

try {
  Get-Command pandoc | Out-Null
} catch {
  Write-Warning "Please install Pandoc - choco install pandoc -y"
  exit 1
}

git submodule update --remote --rebase

# https://github.com/jgm/pandoc/issues/2923
function Convert-MarkdownLinks($text) {

  $text = $text -replace '\[\[([^\|\]]*)\|([^\|\]]*)\]\]', '<a href="$2">$1</a>'
  $text = $text -replace '\[\[([^\|\]]*)\]\]', '<a href="$1">$1</a>'

  #if ($text -match 'href\=\"([A-Z])([^\"])+\"') {
  #  $replaceValue = 'href="' + $matches[1].ToLower() + '$2"'
  #  $text = $text.Replace( $matches[0], $replaceValue)
  #}

  Write-Output $text
}

function Fix-MarkdownConversion($text) {

  $text = $text -creplace 'â€”', '--'
  $text = $text -creplace 'â€“', '–'
  $text = $text -creplace 'â€¦', '...'
  $text = $text -creplace 'â€™', '&#39;'
  $text = $text -creplace 'â€œ', '&quot;'
  $text = $text -creplace 'â€', '&quot;'
  $text = $text -creplace 'Â', '&nbsp;'

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
  & pandoc.exe --from markdown_github --to html5 --old-dashes -V lang="en" -B docgen/header.txt -A docgen/footer.txt -o "$htmlFileName" "$($_.FullName)"

  $fileContent = Convert-ImageUrls (Fix-MarkdownConversion (Convert-MarkdownLinks (Get-Content $htmlFileName).Replace("@","@@").Replace("{{AT}}","@").Replace("{{DocName}}",$docName)))
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
