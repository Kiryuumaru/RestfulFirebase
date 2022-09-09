using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.TestCore;

public static class Credentials
{
    public static FirebaseConfig Config()
    {
        return new("restfulplayground", "AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o");
    }
}
