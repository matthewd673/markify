# markify

Markify automatically generates documentation pages for C# projects in GitHub-flavored markdown.

**DISCLAIMER:** In its current state, this project is nothing more than a quick fix. I needed something to generate GitHub wikis and it does that. It does not do it in a particularly robust or elegant way. But it works for now, and someday it'll be improved.

## Usage

`markify [input directory] [output directory]`

**Additional options:**

- `--print` in place of the output directory, to print the result to console
- `[a filename]` in place of the input directory, to only build a single file