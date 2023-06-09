﻿using System;
using System.Collections.Generic;
using GoOS.Themes;

namespace GoOS.Util;

public class localRepo
{
    private Dictionary<string, string> fileContents;

    public localRepo()
    {
        fileContents = new Dictionary<string, string>();
        fileContents["test"] = "print=test";
    }

    public string GetFile(string name)
    {
        if (fileContents.ContainsKey(name))
        {
            return fileContents[name];
        }
        else
        {
            Kernel.log(ThemeManager.ErrorText,$"LR: Unable to locate {name}!");
            return null;
        }
    }
    
}