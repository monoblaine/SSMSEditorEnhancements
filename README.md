# SSMS Editor Enhancements

This extension adds several editor commands to SQL Server Management Studio 18.x.

## Motivation

I use Visual Studio for front-end/back-end coding and db scripting. SQL Server Data Tools component that comes bundled with Visual Studio is really good and it does the job most of the time. But it also has some bugs (random CPU spikes, table viewer filtering bugs, etc.) that never get fixed. The only reason I sticked to it was some of the Visual Studio extensions that I couldn't live without ([Trailing Whitespace Visualizer](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.TrailingWhitespaceVisualizer) and [Subword Navigation](https://marketplace.visualstudio.com/items?itemName=OlleWestman.SubwordNavigation)) and AFAIK SSMS didn't support extensions out-of-the-box. Then I learned that SSMS actually supports extensions (or maybe a subset of them) becuase it uses Visual Studio Isolated Shell.

## Requirements

* [SQL Server Management Studio 18.x](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

## Commands

### `FixDocumentEncoding` command

Changes the document's encoding to UTF-8 (without BOM) and saves the document.

### `BeautifyCode` command

* Replaces all `[` and `]` characters with `"` (Personal obsession),
* Removes excessive trailing newline characters from EOF,
* Converts certain SQL keywords (`ALTER`, `CREATE`, `DELETE`, `INSERT`, `SELECT`) into lowercase (if they are in the beginning of the line)
* Removes trailing whitespace

### `EndOfWord(Extend)` and `StartOfWord(Extend)` commands

These are my substitudes for `ctrl + →`, `ctrl + shift + →`, `ctrl + ←` and `ctrl + shift + ←`. They use `ITextStructureNavigator.GetExtentOfWord` under the hood.

## Credits

Some of the code in this project was taken from or inspired by the following repositories:

* https://github.com/madskristensen/TrailingWhitespace
* https://github.com/ow--/vs-subword-navigation
* https://github.com/VsVim/VsVim

## Installation

1. Clone the repo.
2. Build it. (preferably using VS2017)
3. Make sure SSMS isn't running.
4. Extract (unzip) `SSMSEditorEnhancements.vsix` into `SSMSEditorEnhancements` and copy that folder into `C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Extensions\`
5. Run SSMS. Commands will be listed under `Tools` menu but you'll probably prefer creating shortcuts for those commands.
