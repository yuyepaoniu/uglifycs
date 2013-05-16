using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglifyCS;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Uglify ss = new Uglify();
        string s=    ss.squeeze_it(@"function(){
var a=1;

}");
        }
    }
}
