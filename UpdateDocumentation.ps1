$ErrorActionPreference = 'Stop'

try {
  Get-Command pandoc | Out-Null
} catch {
  Write-Warning "Please install pandoc - choco install pandoc -y"
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

Get-ChildItem -Path choco.wiki -Recurse -ErrorAction SilentlyContinue -Filter *.md | %{
  $docName = [System.IO.Path]::GetFileNameWithoutExtension($_)
  #$htmlFileName = "docgen\$docName.cshtml"
  $htmlFileName = "chocolatey\Website\Views\Documentation\$docName.cshtml"

  & pandoc.exe --from markdown_github+simple_tables+native_spans+native_divs+multiline_tables --to html5 --old-dashes -V lang="en" -B docgen/header.txt -A docgen/footer.txt -o "$htmlFileName" "$($_.FullName)"

  ($fileContent = Fix-MarkdownConversion (Convert-MarkdownLinks (Get-Content $htmlFileName).Replace("@","@@").Replace("{{AT}}","@").Replace("{{DocName}}",$docName))) | 
    Select -Index (13..$($fileContent.count -3)) | 
    Set-Content $htmlFileName -Force -Encoding UTF8
}