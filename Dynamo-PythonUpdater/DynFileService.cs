using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

namespace Dynamo.PythonUpdater
{
    /// <summary>
    /// Provides static methods to work with Dynamo for Revit files.
    /// </summary>
    internal static class DynFileService
    {
        /// <summary>
        /// Performs a find and replace operation within a Dynamo file.
        /// </summary>
        /// <param name="dynJson">Dynamo file to search in.</param>
        /// <param name="oldString">String to search for.</param>
        /// <param name="newString">String to replace with.</param>
        /// <returns>True if string found and replaced succesfully, otherwise false.</returns>
        internal static bool PyFindAndReplace(string dynJson, string oldString, string newString, string? outputFileName = null)
        {
            int findCount = 0;
            string outputfileName = "";
            const string property = "Code";

            try
            {
                JsonNode? dynNode = JsonNode.Parse(dynJson);
                var encoderSettings = new TextEncoderSettings();
                encoderSettings.AllowCharacters('\u0022', '\u0026', '\u0027');
                encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(encoderSettings),
                    WriteIndented = true
                };

                if (dynNode != null)
                {
                    outputfileName = outputFileName != null ? outputFileName : dynNode["Name"].GetValue<string>() + ".dyn";
                    JsonNode? nodes = dynNode["Nodes"];

                    if (nodes != null)
                    {
                        foreach (JsonObject? node in nodes.AsArray())
                        {
                            // ***** Search for property in every node. *****
                            if (node.TryGetPropertyValue(property, out JsonNode? oldValue))
                            {
                                string source = oldValue.GetValue<string>();

                                if (!string.IsNullOrEmpty(source) && source.Contains(oldString)) 
                                {
                                    string newValue = source.Replace(oldString, newString);
                                    node[property] = newValue;
                                    findCount++;
                                }
                            }
                        }
                    }
                }

                // ***** Save modified DOM to file. *****
                if (findCount > 0)
                {
                    string outputDir = "C:\\Users\\" + Environment.UserName + "\\Downloads\\modified_dynamo";

                    // ***** Check output directory exists
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir); 
                    }

                    string outputFilePath = Path.Combine(outputDir, outputfileName);
                    string jsonString = dynNode.ToJsonString(options);
                    File.WriteAllText(outputFilePath, jsonString);

                    return true;
                }
            }
            catch (JsonException jex)
            {
                Console.WriteLine("Error: {0}", jex.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            return false;
        }

        /// <summary>
        /// Checks if Dynamo file contains any Python Script nodes.
        /// </summary>
        /// <param name="dynJson">The Dynamo file as json string.</param>
        /// <returns>True if it contains Python Script nodes, otherwise returns false.</returns>
        internal static bool ContainsPyNodes(string dynJson)
        {
            using (JsonDocument document = JsonDocument.Parse(dynJson))
            {
                JsonElement root = document.RootElement;
                JsonElement nodes = root.GetProperty("Nodes");

                foreach (JsonElement node in nodes.EnumerateArray())
                {
                    if (node.TryGetProperty("NodeType", out JsonElement type))
                    {
                        if (type.GetString() == "PythonScriptNode")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
