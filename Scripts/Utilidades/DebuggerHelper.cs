using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiveyears3.Scripts.Utilidades
{
    public partial class DebuggerHelper : Node
    {
        public override void _Ready()
        {
            Debugger.Launch();
        }
    }
}
