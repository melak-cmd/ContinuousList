using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;
using System.ComponentModel;

namespace ContinuousList
{
    public class CarouselPanel : Panel, ISurfaceScrollInfo
    {
        #region Constructeur
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public CarouselPanel()
        {
            this.Loaded += new RoutedEventHandler(CarouselPanel_Loaded);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region AutoCenter Events
        //- - - - - - - - - - - - - - - - - - - - - - - -
        void CarouselPanel_Loaded(object sender, RoutedEventArgs e)
        {
            SurfaceScrollViewer ssv = ScrollOwner as SurfaceScrollViewer;
            if (ssv != null)
            {
                PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(SurfaceScrollViewer))["IsScrolling"];
                prop.AddValueChanged(ssv, new EventHandler(this.ScrollEnded));
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void ScrollEnded(object sender, EventArgs e)
        {
            if (autoCenter)
            {
                SurfaceScrollViewer ssv = sender as SurfaceScrollViewer;
                if (ssv != null)
                {
                    if (!ssv.IsScrolling)
                    {
                        this.MoveCenter();
                    }
                }
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region special properties
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected bool initDone = false;
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected int maxSize = 1000000;
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected double carouselPadding = 0;
        protected double angleEcart = 0;
        protected double angleOffset = 0;
        protected double radius = 0;
        protected double angleMin = 0;
        protected double angleMax = 0;
        protected int angleMinIndex = 0;
        protected int angleMaxIndex = 0;
        protected List<double> angleChildren = new List<double>();
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected double _CenterScale = 1;
        public double CenterScale
        {
            get { return _CenterScale; }
            set { _CenterScale = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected double _CenterScaleDuration = 500;
        public double CenterScaleDuration
        {
            get { return _CenterScaleDuration; }
            set { _CenterScaleDuration = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public UIElement activeElement = null;
        public int activeIndex = 0;
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected bool _isVertical = true;
        public bool isVertical
        {
            get { return _isVertical; }
            set { _isVertical = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected int _ItemsCount = 7;
        public int ItemsCount
        {
            get { return _ItemsCount; }
            set
            {
                if (value >= 3)
                {
                    _ItemsCount = value;
                    if (value % 2 == 0) { _ItemsCount--; }
                }
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected bool _autoCenter = true;
        public bool autoCenter
        {
            get { return _autoCenter; }
            set { _autoCenter = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region special methods
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void CarouselReOrder()
        {
            while (angleOffset + Math.PI / 2 + angleEcart / 2 > angleMax)
            {
                angleMax += angleEcart;
                angleChildren[angleMinIndex] = angleMax;
                angleMaxIndex = angleMinIndex;
                angleMinIndex = (angleMinIndex + 1) % InternalChildren.Count;
                angleMin = angleChildren[angleMinIndex];
            }

            while (angleOffset - Math.PI / 2 - angleEcart / 2 < angleMin)
            {
                angleMin -= angleEcart;
                angleChildren[angleMaxIndex] = angleMin;
                angleMinIndex = angleMaxIndex;
                angleMaxIndex = (angleMaxIndex - 1 + InternalChildren.Count) % InternalChildren.Count;
                angleMax = angleChildren[angleMaxIndex];
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void CarouselArrange()
        {
            CarouselArrange(false);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void CarouselArrange(bool animated)
        {
            if (InternalChildren.Count == 0) { return; }

            CarouselReOrder();

            int i = 0;
            int n = InternalChildren.Count;
            int zn = (int)Math.Floor(ItemsCount / 2.0) + 1;
            int prev, next;
            UIElement child = null;
            double angle = 0;
            double x = 0;
            double y = 0;
            double w = 0;
            double h = 0;
            double scale = 1;
            double angleBorne = Math.PI / 2 + angleEcart;

            // determine activeIndex
            for (i = 0; i < n; i++)
            {
                child = InternalChildren[i];
                angle = angleChildren[i] - angleOffset;

                Panel.SetZIndex(child, -1);
                if (angle >= -angleEcart / 2 && angle <= angleEcart / 2)
                {
                    activeElement = child;
                    activeIndex = i;
                }
                else if (angle < -angleBorne || angle > angleBorne)
                {
                    SetTransform(child, 0, 0, 1, animated);
                }
            }

            // z-index
            for (i = 0; i < zn; i++)
            {
                prev = (activeIndex - zn + i + InternalChildren.Count) % InternalChildren.Count;
                next = (activeIndex + zn - i + InternalChildren.Count) % InternalChildren.Count;
                Panel.SetZIndex(InternalChildren[prev], i);
                Panel.SetZIndex(InternalChildren[next], i);
            }
            Panel.SetZIndex(InternalChildren[activeIndex], zn + 1);

            // transforms
            for (i = 0; i < n; i++)
            {
                angle = angleChildren[i] - angleOffset;
                if (angle >= -angleBorne && angle <= angleBorne)
                {
                    child = InternalChildren[i];

                    if (_isVertical)
                    {
                        w = radius * (1 + Math.Cos(angle)) * _viewport.Width / (2 * radius);
                        y = maxSize / 2 + radius * Math.Sin(angle) - child.DesiredSize.Height / 2;
                        x = _viewport.Width / 2 - child.DesiredSize.Width / 2;
                        scale = w / child.DesiredSize.Width;
                        h = scale * child.DesiredSize.Height;
                    }
                    else
                    {
                        h = radius * (1 + Math.Cos(angle)) * _viewport.Height / (2 * radius);
                        x = maxSize / 2 + radius * Math.Sin(angle) - child.DesiredSize.Width / 2;
                        y = _viewport.Height / 2 - child.DesiredSize.Height / 2;
                        scale = h / child.DesiredSize.Height;
                        w = scale * child.DesiredSize.Width;
                    }

                    if (i == activeIndex && ScrollOwner != null)
                    {
                        SetTransform(child, x, y, scale, animated, delegate
                        {
                            ScrollTo(angleOffset);
                        });
                    }
                    else
                    {
                        scale /= CenterScale;
                        SetTransform(child, x, y, scale, animated);
                    }
                }
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void MoveCenter()
        {
            angleOffset = angleChildren[activeIndex];
            CarouselArrange(true);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void Refresh()
        {
            //ScrollOwner.InvalidateMeasure();
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void ScrollTo(double angle)
        {
            double offset = angle * radius + maxSize / 2;
            if (ScrollOwner != null)
            {
                if (_isVertical)
                {
                    ScrollOwner.ScrollToVerticalOffset(offset);
                }
                else
                {
                    ScrollOwner.ScrollToHorizontalOffset(offset);
                }
            }
            else
            {
                CarouselArrange();
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void SetTransform(UIElement child, double X, double Y, double Scale)
        {
            SetTransform(child, X, Y, Scale, false, null);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void SetTransform(UIElement child, double X, double Y, double Scale, bool animated)
        {
            SetTransform(child, X, Y, Scale, animated, null);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected void SetTransform(UIElement child, double X, double Y, double Scale, bool animated, EventHandler endEvent)
        {
            TransformGroup group = child.RenderTransform as TransformGroup;
            ScaleTransform scale;
            TranslateTransform trans;
            if (animated && group != null)
            {
                scale = group.Children[0] as ScaleTransform;
                trans = group.Children[1] as TranslateTransform;

                if (endEvent != null)
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, NewAnimation(scale.ScaleX, Scale, CenterScaleDuration, endEvent));
                }
                else
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, NewAnimation(scale.ScaleX, Scale, CenterScaleDuration));
                }
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, NewAnimation(scale.ScaleY, Scale, CenterScaleDuration));
                trans.BeginAnimation(TranslateTransform.XProperty, NewAnimation(trans.X, X, CenterScaleDuration));
                trans.BeginAnimation(TranslateTransform.YProperty, NewAnimation(trans.Y, Y, CenterScaleDuration));
            }
            else
            {
                group = new TransformGroup();
                scale = new ScaleTransform(Scale, Scale, child.DesiredSize.Width / 2, child.DesiredSize.Height / 2);
                trans = new TranslateTransform(X, Y);
                group.Children.Add(scale);
                group.Children.Add(trans);
                child.RenderTransform = group;
            }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected DoubleAnimation NewAnimation(double from, double to, double duration)
        {
            return NewAnimation(from, to, duration, null);
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected DoubleAnimation NewAnimation(double from, double to, double duration, EventHandler endEvent)
        {
            DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration));
            anim.AccelerationRatio = 0.5;
            anim.DecelerationRatio = 0.5;
            if (endEvent != null)
            {
                anim.Completed += endEvent;
            }
            return anim;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        #endregion

        #region panel Override
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size MeasureOverride(Size availableSize)
        {
            angleChildren.Clear();

            if (double.IsInfinity(availableSize.Height) || double.IsInfinity(availableSize.Width))
            {
                return new Size(1, 1);
            }

            if (InternalChildren.Count == 0)
            {
                return availableSize;
            }

            _viewport = availableSize;

            activeIndex = (int)Math.Floor(InternalChildren.Count / 2.0);
            angleEcart = Math.PI / (ItemsCount - 1);
            double angle = -angleEcart * activeIndex;
            carouselPadding = 0;
            angleMinIndex = 0;
            angleMin = angle;

            if (isVertical)
            {
                // init
                if (!initDone)
                {
                    _extent = new Size(availableSize.Width, maxSize);
                    _viewportOffset = new Point(0, maxSize / 2);
                    RenderTransform = new TranslateTransform(0, -(_extent.Height - _viewport.Height) / 2.0);
                }

                // measure
                double maxWidth = 0;
                foreach (UIElement child in InternalChildren)
                {
                    child.Measure(availableSize);
                    if (child.DesiredSize.Height > carouselPadding)
                    {
                        carouselPadding = child.DesiredSize.Height;
                    }
                    if (child.DesiredSize.Width > maxWidth)
                    {
                        maxWidth = child.DesiredSize.Width;
                    }
                    angleChildren.Add(angle);
                    angle += angleEcart;
                }
                carouselPadding *= availableSize.Width / (2 * maxWidth);
                radius = (_viewport.Height - carouselPadding) / 2;
            }
            else
            {
                // init
                if (!initDone)
                {
                    _extent = new Size(maxSize, availableSize.Height);
                    _viewportOffset = new Point(maxSize / 2, 0);
                    RenderTransform = new TranslateTransform(-(_extent.Width - _viewport.Width) / 2.0, 0);
                }

                // measure
                double maxHeight = 0;
                foreach (UIElement child in InternalChildren)
                {
                    child.Measure(availableSize);
                    if (child.DesiredSize.Width > carouselPadding)
                    {
                        carouselPadding = child.DesiredSize.Width;
                    }
                    if (child.DesiredSize.Height > maxHeight)
                    {
                        maxHeight = child.DesiredSize.Height;
                    }
                    angleChildren.Add(angle);
                    angle += angleEcart;
                }
                carouselPadding *= availableSize.Height / (2 * maxHeight);
                radius = (_viewport.Width - carouselPadding) / 2;
            }

            angleMaxIndex = InternalChildren.Count - 1;
            angleMax = angle - angleEcart;

            initDone = true;

            return availableSize;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected override Size ArrangeOverride(Size finalSize)
        {

            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
            }
            this.CarouselArrange();
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
            int currentIndex = InternalChildren.IndexOf(activeElement);
            if (index != currentIndex)
            {
                angleOffset = angleChildren[index];
                ScrollTo(angleOffset);
            }
            return rectangle;
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        public void SetHorizontalOffset(double offset)
        {
            if (CanHorizontallyScroll && !_isVertical)
            {
                _viewportOffset.X = offset;
                angleOffset = (offset - maxSize / 2) / radius;
                CarouselArrange();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (CanVerticallyScroll && _isVertical)
            {
                _viewportOffset.Y = offset;
                angleOffset = (offset - maxSize / 2) / radius;
                CarouselArrange();
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
        protected bool _CanHorizontallyScroll = false;
        public bool CanHorizontallyScroll
        {
            get { return _CanHorizontallyScroll; }
            set { _CanHorizontallyScroll = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected bool _CanVerticallyScroll = false;
        public bool CanVerticallyScroll
        {
            get { return _CanVerticallyScroll; }
            set { _CanVerticallyScroll = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected Size _extent = new Size(0, 0);
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected ScrollViewer _ScrollOwner;
        public ScrollViewer ScrollOwner
        {
            get { return _ScrollOwner; }
            set { _ScrollOwner = value; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected Point _viewportOffset;
        public double HorizontalOffset
        {
            get { return _viewportOffset.X; }
        }

        public double VerticalOffset
        {
            get { return _viewportOffset.Y; }
        }
        //- - - - - - - - - - - - - - - - - - - - - - - -
        protected Size _viewport = new Size(0, 0);
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
