# Orderstamp

Orderstamp is a command-line simulator for basic timestamp-ordering concurrency control in database transaction histories. It assigns monotonically increasing timestamps to transactions, tracks read and write timestamps for each data item, detects ordering violations, aborts conflicting transactions, and automatically retries transactions aborted by the scheduler.

Histories are entered through an interactive command shell. The simulator can show every operation, pause after each step, print only the final summary, or load a history from a text file.

## Install and run on Windows

1. Download the ZIP from the latest release and extract all its files into one folder.
2. Open the extracted folder and run `Orderstamp.exe`.

## Build

```powershell
dotnet build -c Release
```

To create a distributable folder:

```powershell
dotnet publish src/Orderstamp -c Release -o publish
```

## Usage

Start the interactive shell:

```powershell
dotnet run --project src/Orderstamp
```

The shell displays a `>` prompt. Type `help` to list its commands:

```text
Orderstamp 1.0.0
Type help to see the commands.

> run r1(X); w2(X); r2(Y); w1(X); c1; c2
```

Available commands:

| Command | Description |
| --- | --- |
| `run <history>` | Execute a history with detailed output |
| `step <history>` | Execute and pause after every operation |
| `summary <history>` | Execute and print only the final summary |
| `file <path>` | Execute a history stored in a text file |
| `help` | Show the available commands |
| `clear` | Clear the terminal |
| `version` | Show the program version |
| `exit` | Close the program |

A history may also be entered directly without the `run` command.

## History syntax

Operations may be separated by semicolons, commas, or whitespace.

| Syntax | Meaning |
| --- | --- |
| `bN` | Begin transaction N |
| `rN(X)` | Transaction N reads item X |
| `wN(X)` | Transaction N writes item X |
| `cN` | Commit transaction N |
| `aN` | Abort transaction N |

An explicit begin operation is optional. A transaction begins automatically when its first read or write operation is processed.

## License

This project is licensed under the [MIT License](LICENSE).
