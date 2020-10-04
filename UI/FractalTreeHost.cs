using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FractalTree.UI
{
    public class FractalTreeHost : FrameworkElement
    {
        private VisualCollection _VisualChildren = null;

        private object _BranchLock = new object();

        private HashSet<Tuple<Point, Point>> _BranchCoordinates = new HashSet<Tuple<Point, Point>>();

        protected override int VisualChildrenCount
        {
            get
            {
                return _VisualChildren.Count;
            }
        }

        #region Constructor

        public FractalTreeHost()
        {
            _VisualChildren = new VisualCollection(this);
        }

        #endregion

        #region Method overrides

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index > VisualChildrenCount)
            {
                throw new ArgumentOutOfRangeException("index for visual child out of range");
            }

            return _VisualChildren[index];
        }

        #endregion

        #region Methods

        public void ClearHostChildren()
        {
            _BranchCoordinates.Clear();
            _VisualChildren.Clear();
        }

        public void AddBranchCoordinate(Point x, Point y)
        {
            lock (_BranchLock)
            {
                _BranchCoordinates.Add(new Tuple<Point, Point>(x, y));
            }
        }

        public void AddBranchCoordinate(Tuple<Point, Point> pointPair)
        {
            lock (_BranchLock)
            {
                _BranchCoordinates.Add(pointPair);
            }
        }

        public void RenderTree(double brushWidth)
        {
            Pen renderPen = new Pen(Brushes.Black, brushWidth);
            renderPen.Freeze();

            DrawingVisual visualTree = new DrawingVisual();

            using (DrawingContext drawingContext = visualTree.RenderOpen())
            {
                lock (_BranchLock)
                {
                    foreach (Tuple<Point, Point> line in _BranchCoordinates.ToList())
                    {
                        drawingContext.DrawLine(renderPen, line.Item1, line.Item2);
                    }
                }
            }

            _VisualChildren.Add(visualTree);
        }

        #endregion
    }
}
