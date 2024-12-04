# CSharp BMS Parser
## Versions
- .NET standard 2.0 for legacy Unity (e. g. before 2021.2)
- .NET standard 2.1 (tested on 2022.3.19f1)
## Requirements to use
### Install NuGet packages in non-unity project
- System.Text.Encoding.CodePages
    - For support uncommon encoding (e.g. Shift-JIS)

## Features
- Basic Note output
- Long Note output (LNTYPE 1, LNOBJ only)
- Base64 Parse
- #RANDOM GIMMIK (Sobrem - Random is also works!)
- BPM/EXBPM/STOP GIMMIK
- 4K/6K Extend Command
- MD5/SHA256/SHA512 Hashing Support for IR(Internet Ranking) and illegal BMS files(e. g. copied patterns of IIDX or other commercial games) validation

## Support keys
### BMS
#### Standard
- BEAT-5K
- BEAT-7K
- BEAT-10K
- BEAT-14K
- POPN
#### Non-standard keys
> These keys are not support on major BMS player(e. g. Lunatic Rave 2, beatoraja) yet. so you have to add extend command on .bms file
- BEAT-4K (command: '#4K' / exclude 3, 6, 7 lane)
- BEAT-6K (command: '#6K' / exclude 4 lane)

### BMSON
Not working on it yet. I'll working on it soon.

## How to use
### Parse single BMS file
This method is support .BMS .BME .BML .PMS extensions.
```cs
BMS bmsParser = new BMS();
BMSModel model = bmsParser.Decode("<bms file path>");
// e. g. BMSModel model = bmsParser.Decode("C:\\Aery5Key\\[モリモリあつし] MilK\\_MilK_Aery.bms"); // 5k
// e. g. BMSModel model = bmsParser.Decode("C:\\BOFXVII\\[Clue]Random\\_random_s3.bms"); // 7k + random gimmik
```

### Full example
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