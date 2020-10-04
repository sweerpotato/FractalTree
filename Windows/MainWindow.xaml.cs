using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using PointPair = System.Tuple<System.Windows.Point, System.Windows.Point>;

namespace FractalTree.Windows
{
    public partial class MainWindow : Window
    {
        private Task _BranchTask = null;
        private int _MaxRecursions = -1;
        private double _Length = -1d;
        private double _Width = -1d;
        private double _ShrinkFactor = 1d;

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private async IAsyncEnumerable<PointPair> GetBranches(int branchNumber, double radAngle, double increment, double previousX2, double previousY2, double shrinkFactor)
        {
            if (branchNumber > _MaxRecursions)
            {
                yield break;
            }

            double shrunkLength = _Length / shrinkFactor;

            double rightXEndPoint = previousX2 + shrunkLength * Math.Cos(radAngle - increment);
            double rightYEndPoint = previousY2 + shrunkLength * Math.Sin(radAngle - increment);
            double leftXEndPoint = previousX2 + shrunkLength * Math.Cos(radAngle + increment);
            double leftYEndPoint = previousY2 + shrunkLength * Math.Sin(radAngle + increment);

            ++branchNumber;

            Point branchOrigin = new Point(previousX2, previousY2);

            yield return new PointPair(branchOrigin, new Point(rightXEndPoint, rightYEndPoint));
            yield return new PointPair(branchOrigin, new Point(leftXEndPoint, leftYEndPoint));

            await foreach (PointPair rightBranchPoints in GetBranches(branchNumber, radAngle - increment, increment, rightXEndPoint, rightYEndPoint, shrinkFactor * 1.25))
            {
                yield return rightBranchPoints;
            }

            await foreach (PointPair leftBranchPoints in GetBranches(branchNumber, radAngle + increment, increment, leftXEndPoint, leftYEndPoint, shrinkFactor * 1.25))
            {
                yield return leftBranchPoints;
            }
        }

        private async IAsyncEnumerable<PointPair> GetBranchesAsync(int branchNumber, double radAngle, double increment, double previousX2, double previousY2, double shrinkFactor)
        {
            await foreach (PointPair branch in GetBranches(branchNumber, radAngle, increment, previousX2, previousY2, shrinkFactor))
            {
                yield return branch;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Generic event for when the user drags any of the sliders
        /// Renders the whole tree
        /// </summary>
        private async void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!IsInitialized ||
                    _BranchTask != null && !_BranchTask.IsCompleted)
                {
                    return;
                }

                FractalTree.ClearHostChildren();

                _MaxRecursions = (int)BranchesSlider.Value;
                _Length = LengthSlider.Value;
                _Width = WidthSlider.Value;
                _ShrinkFactor = ShrinkFactorSlider.Value;

                double increment = (Math.PI / 180) * AngleSlider.Value;

                double x2 = 0;
                double y2 = _Length;

                FractalTree.AddBranchCoordinate(new Point(0, 0), new Point(x2, y2));

                _BranchTask = Task.Factory.StartNew(async () =>
                {
                    await foreach (PointPair branchPoints in GetBranchesAsync(0, Math.PI / 2, increment, x2, y2, _ShrinkFactor))
                    {
                        FractalTree.AddBranchCoordinate(branchPoints);
                    }

                    await Application.Current.Dispatcher.BeginInvoke((Action)(() => FractalTree.RenderTree(_Width)));
                });

                await _BranchTask;
            }
            catch
            {

            }
        }

        #endregion
    }
}
