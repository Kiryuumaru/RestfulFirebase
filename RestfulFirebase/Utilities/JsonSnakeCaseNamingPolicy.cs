﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace RestfulFirebase.Utilities;

internal class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (name == null)
        {
            ArgumentNullException.ThrowIfNull(name);
        }
        if (name.Length < 2)
        {
            return name;
        }
        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(name[0]));
        for (int i = 1; i < name.Length; ++i)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
