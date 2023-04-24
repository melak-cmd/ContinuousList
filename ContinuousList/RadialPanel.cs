using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ContinuousList
{
    public class RadialPanel : Panel
    {
        private int _angleStart = 0;
        private int _angleEnd = 360;
        private bool _itemsIncluded = false;
        private bool _rotate = false;
        private double _rotateAngle = 0;
        private double _radiusPercentage = 100;

        public int angleEnd
        {
            get { return _angleEnd; }
            set { if (value > _angleStart) { _angleEnd = value; this.UpdateLayout(); } }
        }

        public int angleStart
        {
            get { return _angleStart; }
            set { if (value < _angleEnd) { _angleStart = value; this.UpdateLayout(); } }
        }

        public bool itemsIncluded
        {
            get { return _itemsIncluded; }
            set { _itemsIncluded = value; this.UpdateLayout(); }
        }

        public bool rotate
        {
            get { return _rotate; }
            set { _rotate = value; this.UpdateLayout(); }
        }

        public double rotateAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; this.UpdateLayout(); }
        }

        public double radiusPercentage
        {
            get { return _radiusPercentage; }
            set { _radiusPercentage = value; this.UpdateLayout(); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement elem in Children)
            {
                //Give Infinite size as the avaiable size for all the children
                elem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
            return base.MeasureOverride(availableSize);
        }

        //Arrange all children based on the geometric equations for the circle.
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
            {
                return finalSize;
            }
            double angle = _angleStart;

            //Degrees converted to Radian by multiplying with PI/180
            double angle_spacing = 0;

            if (_angleEnd - _angleStart < 360)
            {
                if (_itemsIncluded)
                {
                    angle_spacing = (_angleEnd - _angleStart) / (Children.Count + 1);
                    angle += angle_spacing;
                }
                else
                {
                    if (Children.Count > 1)
                    {
                        angle_spacing = (_angleEnd - _angleStart) / (Children.Count - 1);
                    }
                    else
                    {
                        angle_spacing = (_angleEnd - _angleStart) / 2;
                        angle += angle_spacing;
                    }
                }
            }
            else
            {
                angle_spacing = 360.0 / Children.Count;
            }

            //An approximate radii based on the avialable size , obviusly a better approach is needed here.
            double radiusX = finalSize.Width * radiusPercentage / 200;
            double radiusY = finalSize.Height * radiusPercentage / 200;


            foreach (UIElement elem in Children)
            {
                //Offsetting the point to the Avalable rectangular area which is FinalSize.
                Point actualChildPoint = new Point(
                    finalSize.Width / 2 + Math.Cos(angle * (Math.PI / 180)) * radiusX,
                    finalSize.Height / 2 - Math.Sin(angle * (Math.PI / 180)) * radiusY
                );

                // rotation
                if (this._rotate)
                {
                    elem.RenderTransform = new RotateTransform(-angle + _rotateAngle, elem.DesiredSize.Width / 2, elem.DesiredSize.Height / 2);
                }

                //Call Arrange method on the child element by giving the calculated point as the placementPoint.
                elem.Arrange(new Rect(
                    actualChildPoint.X - elem.DesiredSize.Width / 2,
                    actualChildPoint.Y - elem.DesiredSize.Height / 2,
                    elem.DesiredSize.Width,
                    elem.DesiredSize.Height
                ));

                //Calculate the new _angle for the next element
                angle += angle_spacing;
            }
            return finalSize;
        }

    }
}
