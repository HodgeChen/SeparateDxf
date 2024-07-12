using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeparateDXF.models
{
    internal class LoopTopItem
    {

        public LoopItem cadLoopItem { get; set; }
        public List<LoopTopItem> subLoopTopItemList { get; set; }
        public LoopTopItem(LoopItem cadloopitem)
        {
            cadLoopItem = cadloopitem;
            subLoopTopItemList = new List<LoopTopItem>();
        }

    }
}
