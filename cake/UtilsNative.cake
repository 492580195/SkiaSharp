
var RunGyp = new Action<string, string> ((defines, generators) =>
{
    SetEnvironmentVariable ("GYP_GENERATORS", generators);
    SetEnvironmentVariable ("GYP_DEFINES", defines);
    
    Information ("Running 'sync-and-gyp'...");
    Information ("\tGYP_GENERATORS = " + EnvironmentVariable ("GYP_GENERATORS"));
    Information ("\tGYP_DEFINES = " + EnvironmentVariable ("GYP_DEFINES"));
    
    var result = StartProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath("bin/sync-and-gyp").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
    if (result != 0) {
        throw new Exception ("sync-and-gyp failed with error: " + result);
    }
});

var RunInstallNameTool = new Action<DirectoryPath, string, string, FilePath> ((directory, oldName, newName, library) =>
{
    if (!IsRunningOnUnix ()) {
        throw new InvalidOperationException ("install_name_tool is only available on Unix.");
    }
    
    StartProcess ("install_name_tool", new ProcessSettings {
        Arguments = string.Format("-change {0} {1} \"{2}\"", oldName, newName, library),
        WorkingDirectory = directory,
    });
});

var RunLipo = new Action<DirectoryPath, FilePath, FilePath[]> ((directory, output, inputs) =>
{
    if (!IsRunningOnUnix ()) {
        throw new InvalidOperationException ("lipo is only available on Unix.");
    }
    
    var dir = directory.CombineWithFilePath (output).GetDirectory ();
    if (!DirectoryExists (dir)) {
        CreateDirectory (dir);
    }

    var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
    StartProcess ("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
    });
});
