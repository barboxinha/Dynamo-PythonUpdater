using Dynamo.PythonUpdater;

// ***** Prompt user to input directory path. *****
Console.WriteLine("Please provide a directory path to edit Dynamo files in...");
string? dirPath = Console.ReadLine();

// ***** Validate directory path from user. *****
while (!Directory.Exists(dirPath))
{
    Console.WriteLine("\r\nError: The path provided does not exist or is invalid.");
    Console.WriteLine("Would you like to try again? Y / N");
    string? userChoice = Console.ReadLine();

    if ((!string.IsNullOrEmpty(userChoice)) && (userChoice.ToUpper() == "Y"))
    {
        Console.WriteLine("");
        Console.WriteLine("Please provide a directory path to edit Dynamo files in...");
        dirPath = Console.ReadLine();
        Console.WriteLine("");
    }
    else 
    {
        return;
    }
}

// ***** Prompt user for find and replace strings. *****
Console.WriteLine("\r\nWould you like to search files in this directory only? Press 'Enter'");
Console.WriteLine("OR search files in all folders within directory? Type 'A'");
string? searchChoice = Console.ReadLine();
SearchOption searchOption = string.IsNullOrEmpty(searchChoice) || searchChoice.ToUpper() != "A"
                            ? SearchOption.TopDirectoryOnly
                            : SearchOption.AllDirectories;

// ***** Operate on folder files. *****
DirectoryInfo dir = new DirectoryInfo(dirPath);
FileInfo[] files = dir.GetFiles("*.dyn", searchOption);
int modified = 0;

if (files.Length > 0)
{
    // ***** Prompt user for find and replace strings. *****
    Console.WriteLine($"\r\nInput text to search for...");
    string? oldString = Console.ReadLine();
    Console.WriteLine($"\r\nInput text to replace with...");
    string? newString = Console.ReadLine();

    foreach (FileInfo file in files)
    {
        string dynPath = file.FullName;
        string dynJson = File.ReadAllText(dynPath);

        if (DynFileService.ContainsPyNodes(dynJson) && !string.IsNullOrEmpty(oldString))
        {
            // ***** Do file edits *****
            if (DynFileService.PyFindAndReplace(dynJson, oldString, newString, file.Name))
            {
                modified++;

                if (modified == 1)
                {
                    Console.WriteLine($"\r\n\r\nUpdated the python nodes in the following Dynamo files:");
                    Console.WriteLine($"---------------------------------------------------");
                    Console.WriteLine("");
                }

                Console.WriteLine($"\t--> {file.Name}");  
            }
        }
    }

    Console.WriteLine("");
    Console.WriteLine($"Modified {modified} of {files.Length} files.");

    if (modified > 0)
    {
        Console.WriteLine("Files have been placed under your 'Downloads' folder."); 
    }
}
else
{
    Console.WriteLine("\r\nNo Dynamo files were found in the provided directory.");
}

Console.WriteLine("\r\nPress any key to exit...");
Console.ReadKey();
