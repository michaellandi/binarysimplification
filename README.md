# Binary Simplification

A command-line tool that minimizes Boolean functions using the [Quine–McCluskey algorithm](https://en.wikipedia.org/wiki/Quine%E2%80%93McCluskey_algorithm). Given a list of minterms, it produces a minimized sum-of-products expression — the same process done by hand in logic design courses, but automated.

Written in C# (.NET), originally built as a Computer Architecture class project.

## Usage

Run the executable and enter your minterms as a space-separated list when prompted:

```
Boolean Simplification Program

ENTER MINTERMS: 0 1 3 7 8 9 11 15
```

**Output:**
```
SIMPLIFIED EXPRESSION:
A'B' + AB + BC
```

Minterms must be non-negative integers with no duplicates.

## How It Works

The Quine–McCluskey algorithm is a tabular method for minimizing Boolean expressions:

1. **Group** minterms by the number of `1` bits in their binary representation
2. **Combine** adjacent groups — any two minterms differing by exactly one bit are merged into a prime implicant (the differing bit becomes a don't-care `-`)
3. **Repeat** until no further combinations are possible
4. **Build a prime implicant chart** to find which implicants are essential
5. **Select** the smallest set of implicants that covers all original minterms
6. **Output** the result as a sum-of-products expression using variable names A, B, C, ...

Each pass prints an intermediate matrix showing which implicants cover which minterms.

## Building

Requires Visual Studio or the .NET SDK:

```bash
dotnet build Simplification.csproj
dotnet run
```

Or open `Simplification.csproj` in Visual Studio and run from there.

## License

GNU General Public License v3. See [COPYING](COPYING) for details.
