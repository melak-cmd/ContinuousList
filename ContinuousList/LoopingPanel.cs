using System;
using System.Collections.Generic;
using Microsoft.Surface.Presentation.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ContinuousList
{
    public class LoopingPanel : Panel, ISurfaceScrollInfo
    {
        private int maxSize = 1000000;

        private double totalContentHeight = 0;
        private double totalContentWidth = 0;
        private List<double> PositionChildren = new List<double>();
        private double PositionMin = 0;
        private double PositionMax = 0;
        private int PositionMinIndex = 0;
        private int PositionMaxIndex = 0;

        private bool _isVertical = true;

        public LoopingPanel()
        {
            this.RenderTransform = new TranslateTransform();
        }

        public bool isVertical
        {
            get { return _isVertical; }
            set { _isVertical = value; }
        }

        #region special methods

        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void PositionReOrderArrange(int index)
        {
            if (_isVertical)
            {
                InternalChildren[index].Arrange(new Rect(0, PositionChildren[index], InternalChildren[index].DesiredSize.Width, InternalChildren[index].DesiredSize.Height));
            }
            else
            {
                InternalChildren[index].Arrange(new Rect(PositionChildren[index], 0, InternalChildren[index].DesiredSize.Width, InternalChildren[index].DesiredSize.Height));
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void PositionReOrder()
        {
            double bound1, bound2;
            if (_isVertical)
            {
                bound1 = _viewportOffset.Y + _viewport.Height / 2;
                bound2 = _viewportOffset.Y - _viewport.Height / 2;
            }
            else
            {
                bound1 = _viewportOffset.X + _viewport.Width / 2;
                bound2 = _viewportOffset.X - _viewport.Width / 2;
            }

            while (bound1 > PositionMax)
            {
                PositionChildren[PositionMinIndex] = PositionMax;
                PositionMax += InternalChildren[PositionMinIndex].DesiredSize.Width;
                PositionMaxIndex = PositionMinIndex;
                PositionMinIndex = (PositionMinIndex + 1) % InternalChildren.Count;
                PositionMin = PositionChildren[PositionMinIndex];
                PositionReOrderArrange(PositionMaxIndex);
            }

            while (bound2 < PositionMin)
            {
                PositionMin -= InternalChildren[PositionMaxIndex].DesiredSize.Width;
                PositionChildren[PositionMaxIndex] = PositionMin;
                PositionMinIndex = PositionMaxIndex;
                PositionMaxIndex = (PositionMaxIndex - 1 + InternalChildren.Count) % InternalChildren.Count;
                PositionMax = PositionChildren[PositionMaxIndex];
                PositionReOrderArrange(PositionMinIndex);
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region panel Override
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size MeasureOverride(Size availableSize)
        {
            PositionChildren.Clear();

            if (double.IsInfinity(availableSize.Height) || double.IsInfinity(availableSize.Width))
            {
                return new Size(1, 1);
            }

            if (this.InternalChildren.Count == 0)
            {
                return availableSize;
            }

            _viewport = availableSize;

            if (_isVertical)
            {
                totalContentHeight = 0;
                foreach (UIElement child in this.InternalChildren)
                {
                    child.Measure(availableSize);
                    totalContentHeight += child.DesiredSize.Height;
                }

                _viewportOffset = new Point(0, maxSize / 2);

                if (totalContentHeight < _viewport.Height)
                {
                    _extent = new Size(_viewport.Width, _viewport.Height);
                    CanVerticallyScroll = false;
                    ((TranslateTransform)RenderTransform).Y = 0;
                }
                else
                {
                    _extent = new Size(_viewport.Width, Math.Max(maxSize, totalContentHeight * 15));
                    CanVerticallyScroll = true;
                    ((TranslateTransform)RenderTransform).Y = -maxSize / 2 + _viewport.Height / 2;
                }

                double startPosition = _extent.Height / 2 - totalContentHeight / 2;
                PositionMin = startPosition;
                PositionMinIndex = 0;
                foreach (UIElement child in this.InternalChildren)
                {
                    PositionChildren.Add(startPosition);
                    startPosition += child.DesiredSize.Height;
                }
                PositionMax = startPosition;
                PositionMaxIndex = PositionChildren.Count - 1;
            }
            else
            {
                totalContentWidth = 0;
                foreach (UIElement child in this.InternalChildren)
                {
                    child.Measure(availableSize);
                    totalContentWidth += child.DesiredSize.Width;
                }

                _viewportOffset = new Point(maxSize / 2, 0);

                if (totalContentWidth < _viewport.Width)
                {
                    _extent = new Size(_viewport.Width, _viewport.Height);
                    CanHorizontallyScroll = false;
                    ((TranslateTransform)RenderTransform).X = 0;
                }
                else
                {
                    _extent = new Size(Math.Max(maxSize, totalContentWidth * 15), _viewport.Height);
                    CanHorizontallyScroll = true;
                    ((TranslateTransform)RenderTransform).X = -maxSize / 2 + _viewport.Width / 2;
                }

                double startPosition = _extent.Width / 2 - totalContentWidth / 2;
                PositionMin = startPosition;
                PositionMinIndex = 0;
                foreach (UIElement child in this.InternalChildren)
                {
                    PositionChildren.Add(startPosition);
                    startPosition += child.DesiredSize.Width;
                }
                PositionMax = startPosition;
                PositionMaxIndex = PositionChildren.Count - 1;
            }


            return availableSize;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement child = null;
            int i = 0;
            int n = PositionChildren.Count;

            if (_isVertical)
            {
                for (i = 0; i < n; i++)
                {
                    child = InternalChildren[i];
                    child.Arrange(new Rect(0, PositionChildren[i], child.DesiredSize.Width, child.DesiredSize.Height));
                }
            }
            else
            {
                for (i = 0; i < n; i++)
                {
                    child = InternalChildren[i];
                    child.Arrange(new Rect(PositionChildren[i], 0, child.DesiredSize.Width, child.DesiredSize.Height));
                }
            }

            if (_ScrollOwner != null)
            {
                _ScrollOwner.InvalidateScrollInfo();
            }
            return finalSize;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region ISurfaceScrollInfo Membres
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public Vector ConvertFromViewportUnits(Point origin, Vector offset)
        {
            return offset;
        }

        public Vector ConvertToViewportUnits(Point origin, Vector offset)
        {
            return offset;
        }

        public Vector ConvertToViewportUnitsForFlick(Point origin, Vector offset)
        {
            return offset;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region IScrollInfo Membres
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            int index = InternalChildren.IndexOf((UIElement)visual);
            if (_isVertical)
            {
                SetVerticalOffset(PositionChildren[index]);
            }
            else
            {
                SetHorizontalOffset(PositionChildren[index]);
            }
            return rectangle;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void SetHorizontalOffset(double offset)
        {
            if (CanHorizontallyScroll && !_isVertical)
            {
                _viewportOffset.X = offset;
                ((TranslateTransform)this.RenderTransform).X = -offset + _viewport.Width / 2;
                PositionReOrder();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (CanVerticallyScroll && _isVertical)
            {
                _viewportOffset.Y = offset;
                ((TranslateTransform)this.RenderTransform).Y = -offset + _viewport.Height / 2;
                PositionReOrder();
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region IScrollInfo Membres (partie purement declarative)
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void LineLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - 1);
        }

        public void LineRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + 1);
        }

        public void LineDown()
        {
            SetVerticalOffset(this.VerticalOffset + 1);
        }

        public void LineUp()
        {
            SetVerticalOffset(this.VerticalOffset - 1);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void PageLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - 10);
        }

        public void PageRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + 10);
        }

        public void PageDown()
        {
            SetVerticalOffset(this.VerticalOffset + 10);
        }

        public void PageUp()
        {
            SetVerticalOffset(this.VerticalOffset - 10);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void MouseWheelLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - 10);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + 10);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(this.VerticalOffset + 10);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(this.VerticalOffset - 10);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private bool _CanHorizontallyScroll = false;
        public bool CanHorizontallyScroll
        {
            get { return _CanHorizontallyScroll; }
            set { _CanHorizontallyScroll = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private bool _CanVerticallyScroll = false;
        public bool CanVerticallyScroll
        {
            get { return _CanVerticallyScroll; }
            set { _CanVerticallyScroll = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private Size _extent = new Size(0, 0);
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private ScrollViewer _ScrollOwner;
        public ScrollViewer ScrollOwner
        {
            get { return _ScrollOwner; }
            set { _ScrollOwner = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private Point _viewportOffset;
        public double HorizontalOffset
        {
            get { return _viewportOffset.X; }
        }

        public double VerticalOffset
        {
            get { return _viewportOffset.Y; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        private Size _viewport = new Size(0, 0);
        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion
    }
}
