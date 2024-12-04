# CSharp BMS Parser
## Versions
- .NET standard 2.0 for legacy Unity like before 2021.2 but.. are you sure use theese versions?
- .NET standard 2.1 (tested on 2022.3.19f1)
## Requirements to use
### Install nuget packages in non-unity project
- System.Text.Encoding.CodePages
    - For support uncommon encoding like Shift-JIS.

## Features
- Basic Note output
- Long Note output (LNTYPE 1, LNOBJ only)
- Base64 Parse
- #RANDOM GIMMIK (Sobrem - Random is also works!)
- BPM/EXBPM/STOP GIMMIK
- 4K/6K Extend Command
- MD5/SHA256/SHA512 Hashing Support for IR(Internet Ranking) and illegal BMS files (like a copied patterns of IIDX or other commercial games) validation

## How to use
```cs
using BMSParser

namespace example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BMS bmsParser = new BMS();
            BMSModel model = bmsParser.Decode("<bms file path>");

            // HEADERS //
            Console.WriteLine(model.Extension);
            Console.WriteLine(model.Player);
            Console.WriteLine(model.Rank);
            Console.WriteLine(model.Genre);
            Console.WriteLine(model.Title);
            Console.WriteLine(model.SubTitle);
            Console.WriteLine(model.Artist);
            Console.WriteLine(model.SubArtist); 
            Console.WriteLine(model.BackBmp);
            Console.WriteLine(model.Banner);    
            Console.WriteLine(model.StageFile);
            Console.WriteLine(model.Base);
            Console.WriteLine(model.Lnobj);
            Console.WriteLine(model.Mode);  
            Console.WriteLine(model.LnType);
            Console.WriteLine(model.MD5);
            Console.WriteLine(model.SHA256);
            Console.WriteLine(model.SHA512);

            // TIMESTAMP //
            foreach (KeyValuePair<double, Timestamp> timestamp in model.Timestamp)
            {
                Console.WriteLine(timestamp.Value.Time);
            }
        }
    }
} 
```

## TODO
- [ ] BMSON Parse
- [ ] LNTYPE 2 support