using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SeparateDXF.models
{
    internal class RectBox
    {

        public double Min_X { get; set; }
        public double Min_Y { get; set; }
        public double Max_X { get; set; }
        public double Max_Y { get; set; }

        public RectBox()
        {
            Min_X = double.NaN;
            Min_Y = double.NaN;
            Max_X = double.NaN;
            Max_Y = double.NaN;
        }
        public RectBox(double minX, double minY, double maxX, double maxY)
        {
            Min_X = minX;
            Min_Y = minY;
            Max_X = maxX;
            Max_Y = maxY;
        }

        public void SetRect(double minX, double minY, double maxX, double maxY)
        {
            Min_X = minX;
            Min_Y = minY;
            Max_X = maxX;
            Max_Y = maxY;
        }

        public void Unin(RectBox rectbox)
        {
            Min_X = Min_X < rectbox.Min_X ? Min_X : rectbox.Min_X;
            Min_Y = Min_Y < rectbox.Min_Y ? Min_Y : rectbox.Min_Y;
            Max_X = Max_X > rectbox.Max_X ? Max_X : rectbox.Max_X;
            Max_Y = Max_Y > rectbox.Max_Y ? Max_Y : rectbox.Max_Y;
        }
        public (double size_X, double size_Y) Size()
        {
            return (Max_X - Min_X, Max_Y - Min_Y);
        }
        public void Expand(double val)
        {
            Min_X -= val;
            Min_Y -= val;
            Max_X += val;
            Max_Y += val;
        }

        public bool Include(RectBox rectbox)
        {
            if (Min_X < rectbox.Min_X &&
                Min_Y < rectbox.Min_Y &&
                Max_X > rectbox.Max_X &&
                Max_Y > rectbox.Max_Y
                )
            {
                return true;

            }
            else
            {
                return false;
            }
        }



        public bool Iscross(RectBox rectbox, double TOL = 0.0)
        {
            //判断两个矩形是否相交
            //(x1,y1) (x2,y2)为第一个矩形左下和右上角的两个点
            //(x3,y3) (x4,y4)为第二个矩形左下角和右上角的两个点
            //max(x1, x3) <= min(x2, x4) and max(y1, y3) <= min(y2, y4)
            RectBox desrect = new RectBox(rectbox.Min_X - TOL, rectbox.Min_Y - TOL, rectbox.Max_X + TOL, rectbox.Max_Y + TOL);

            if (Math.Max(Min_X, desrect.Min_X) <= Math.Min(Max_X, desrect.Max_X) &&
                Math.Max(Min_Y, desrect.Min_Y) <= Math.Min(Max_Y, desrect.Max_Y))
            { return true; }
            else
            { return false; }
        }

        public bool IsNull()
        {
            if (double.IsNaN(Min_X) || double.IsNaN(Max_X) || double.IsNaN(Min_Y) || double.IsNaN(Max_Y))
            { return true; }
            else
            { return false; }
        }

    }




}
