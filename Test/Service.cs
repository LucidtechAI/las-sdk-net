using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Test.Service
{

    public static class Util {
    
        public static string ResourceId(string resourceName){
            return $"las:{resourceName}:{Guid.NewGuid().ToString().Replace("-", "")}";
        }
    }
}
