﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineViewModel : PropertyChangedBase, IViewAware, IDisposable
    {
        private readonly LayerPropertiesViewModel _layerPropertiesViewModel;
        private readonly IProfileEditorService _profileEditorService;
        private RectangleGeometry _selectionRectangle;

        public TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups, IProfileEditorService profileEditorService)
        {
            _layerPropertiesViewModel = layerPropertiesViewModel;
            _profileEditorService = profileEditorService;

            LayerPropertyGroups = layerPropertyGroups;
            SelectionRectangle = new RectangleGeometry();
            SelectedProfileElement = layerPropertiesViewModel.SelectedProfileElement;

            SelectedProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;
            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;

            Update();
        }

        public RenderProfileElement SelectedProfileElement { get; set; }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }

        public RectangleGeometry SelectionRectangle
        {
            get => _selectionRectangle;
            set => SetAndNotify(ref _selectionRectangle, value);
        }

        public double StartSegmentWidth => _profileEditorService.PixelsPerSecond * SelectedProfileElement.StartSegmentLength.TotalSeconds;
        public double StartSegmentEndPosition => StartSegmentWidth;
        public double MainSegmentWidth => _profileEditorService.PixelsPerSecond * SelectedProfileElement.MainSegmentLength.TotalSeconds;
        public double MainSegmentEndPosition => StartSegmentWidth + MainSegmentWidth;
        public double EndSegmentWidth => _profileEditorService.PixelsPerSecond * SelectedProfileElement.EndSegmentLength.TotalSeconds;
        public double EndSegmentEndPosition => StartSegmentWidth + MainSegmentWidth + EndSegmentWidth;
        public double TotalTimelineWidth => _profileEditorService.PixelsPerSecond * SelectedProfileElement.TimelineLength.TotalSeconds;

        public bool StartSegmentEnabled => SelectedProfileElement.StartSegmentLength != TimeSpan.Zero;
        public bool EndSegmentEnabled => SelectedProfileElement.EndSegmentLength != TimeSpan.Zero;

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            SelectedProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
        }

        private void SelectedProfileElementOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_profileEditorService.SelectedProfileElement.StartSegmentLength))
            {
                NotifyOfPropertyChange(nameof(StartSegmentWidth));
                NotifyOfPropertyChange(nameof(StartSegmentEndPosition));
                NotifyOfPropertyChange(nameof(MainSegmentEndPosition));
                NotifyOfPropertyChange(nameof(EndSegmentEndPosition));
                NotifyOfPropertyChange(nameof(StartSegmentEnabled));
                NotifyOfPropertyChange(nameof(TotalTimelineWidth));
            }
            else if (e.PropertyName == nameof(_profileEditorService.SelectedProfileElement.MainSegmentLength))
            {
                NotifyOfPropertyChange(nameof(MainSegmentWidth));
                NotifyOfPropertyChange(nameof(MainSegmentEndPosition));
                NotifyOfPropertyChange(nameof(EndSegmentEndPosition));
                NotifyOfPropertyChange(nameof(TotalTimelineWidth));
            }
            else if (e.PropertyName == nameof(_profileEditorService.SelectedProfileElement.EndSegmentLength))
            {
                NotifyOfPropertyChange(nameof(EndSegmentWidth));
                NotifyOfPropertyChange(nameof(EndSegmentEndPosition));
                NotifyOfPropertyChange(nameof(EndSegmentEnabled));
                NotifyOfPropertyChange(nameof(TotalTimelineWidth));
            }
        }

        public void Update()
        {
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
            {
                layerPropertyGroupViewModel.TimelinePropertyGroupViewModel.UpdateKeyframes();

                foreach (var layerPropertyBaseViewModel in layerPropertyGroupViewModel.GetAllChildren())
                {
                    if (layerPropertyBaseViewModel is LayerPropertyViewModel layerPropertyViewModel)
                        layerPropertyViewModel.TimelinePropertyBaseViewModel.UpdateKeyframes();
                }
            }
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(StartSegmentWidth));
            NotifyOfPropertyChange(nameof(StartSegmentEndPosition));
            NotifyOfPropertyChange(nameof(MainSegmentWidth));
            NotifyOfPropertyChange(nameof(MainSegmentEndPosition));
            NotifyOfPropertyChange(nameof(EndSegmentWidth));
            NotifyOfPropertyChange(nameof(EndSegmentEndPosition));
            NotifyOfPropertyChange(nameof(TotalTimelineWidth));
        }

        #region Command handlers

        public void KeyframeMouseDown(object sender, MouseButtonEventArgs e)
        {
            // if (e.LeftButton == MouseButtonState.Released)
            //     return;

            var viewModel = (sender as Ellipse)?.DataContext as TimelineKeyframeViewModel;
            if (viewModel == null)
                return;

            ((IInputElement) sender).CaptureMouse();
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && !viewModel.IsSelected)
                SelectKeyframe(viewModel, true, false);
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                SelectKeyframe(viewModel, false, true);
            else if (!viewModel.IsSelected)
                SelectKeyframe(viewModel, false, false);

            e.Handled = true;
        }

        public void KeyframeMouseUp(object sender, MouseButtonEventArgs e)
        {
            _profileEditorService.UpdateSelectedProfileElement();
            ReleaseSelectedKeyframes();

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void KeyframeMouseMove(object sender, MouseEventArgs e)
        {
            var viewModel = (sender as Ellipse)?.DataContext as TimelineKeyframeViewModel;
            if (viewModel == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
                MoveSelectedKeyframes(GetCursorTime(e.GetPosition(View)), viewModel);

            e.Handled = true;
        }

        #region Context menu actions

        public void ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var viewModel = (sender as Ellipse)?.DataContext as TimelineKeyframeViewModel;
            viewModel?.CreateEasingViewModels();
        }

        public void ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            var viewModel = (sender as Ellipse)?.DataContext as TimelineKeyframeViewModel;
            viewModel?.EasingViewModels.Clear();
        }

        public void Copy(TimelineKeyframeViewModel viewModel)
        {
            viewModel.Copy();
        }

        public void Delete(TimelineKeyframeViewModel viewModel)
        {
            viewModel.Delete();
        }

        #endregion

        private TimeSpan GetCursorTime(Point position)
        {
            // Get the parent grid, need that for our position
            var x = Math.Max(0, position.X);
            var time = TimeSpan.FromSeconds(x / _profileEditorService.PixelsPerSecond);

            // Round the time to something that fits the current zoom level
            if (_profileEditorService.PixelsPerSecond < 200)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 5.0) * 5.0);
            else if (_profileEditorService.PixelsPerSecond < 500)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 2.0) * 2.0);
            else
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds));

            // If shift is held, snap to the current time
            // Take a tolerance of 5 pixels (half a keyframe width)
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var tolerance = 1000f / _profileEditorService.PixelsPerSecond * 5;
                if (Math.Abs(_profileEditorService.CurrentTime.TotalMilliseconds - time.TotalMilliseconds) < tolerance)
                    time = _profileEditorService.CurrentTime;
                else if (Math.Abs(_profileEditorService.SelectedProfileElement.StartSegmentLength.TotalMilliseconds - time.TotalMilliseconds) < tolerance)
                    time = _profileEditorService.SelectedProfileElement.StartSegmentLength;
            }

            return time;
        }

        #endregion

        #region Keyframe movement

        public void MoveSelectedKeyframes(TimeSpan cursorTime, TimelineKeyframeViewModel sourceKeyframeViewModel)
        {
            // Ensure the selection rectangle doesn't show, the view isn't aware of different types of dragging
            SelectionRectangle.Rect = new Rect();

            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.SaveOffsetToKeyframe(sourceKeyframeViewModel);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                cursorTime = _profileEditorService.SnapToTimeline(
                    cursorTime,
                    TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 5),
                    true,
                    false,
                    true,
                    sourceKeyframeViewModel.BaseLayerPropertyKeyframe
                );
            }

            sourceKeyframeViewModel.UpdatePosition(cursorTime);

            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ApplyOffsetToKeyframe(sourceKeyframeViewModel);

            _layerPropertiesViewModel.ProfileEditorService.UpdateProfilePreview();
        }


        public void ReleaseSelectedKeyframes()
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            foreach (var keyframeViewModel in keyframeViewModels.Where(k => k.IsSelected))
                keyframeViewModel.ReleaseMovement();
        }

        #endregion

        #region Keyframe selection

        private Point _mouseDragStartPoint;
        private bool _mouseDragging;

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            ((IInputElement) sender).CaptureMouse();

            SelectionRectangle.Rect = new Rect();
            _mouseDragStartPoint = e.GetPosition((IInputElement) sender);
            _mouseDragging = true;
            e.Handled = true;
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void TimelineCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (!_mouseDragging)
                return;

            var position = e.GetPosition((IInputElement) sender);
            var selectedRect = new Rect(_mouseDragStartPoint, position);
            SelectionRectangle.Rect = selectedRect;

            var keyframeViewModels = GetAllKeyframeViewModels();
            var selectedKeyframes = HitTestUtilities.GetHitViewModels<TimelineKeyframeViewModel>((Visual) sender, SelectionRectangle);
            foreach (var keyframeViewModel in keyframeViewModels)
                keyframeViewModel.IsSelected = selectedKeyframes.Contains(keyframeViewModel);

            _mouseDragging = false;
            e.Handled = true;
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition((IInputElement) sender);
                var selectedRect = new Rect(_mouseDragStartPoint, position);
                SelectionRectangle.Rect = selectedRect;
                e.Handled = true;
            }
        }

        public void SelectKeyframe(TimelineKeyframeViewModel clicked, bool selectBetween, bool toggle)
        {
            var keyframeViewModels = GetAllKeyframeViewModels();
            if (selectBetween)
            {
                var selectedIndex = keyframeViewModels.FindIndex(k => k.IsSelected);
                // If nothing is selected, select only the clicked
                if (selectedIndex == -1)
                {
                    clicked.IsSelected = true;
                    return;
                }

                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;

                var clickedIndex = keyframeViewModels.IndexOf(clicked);
                if (clickedIndex < selectedIndex)
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(clickedIndex).Take(selectedIndex - clickedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
                else
                {
                    foreach (var keyframeViewModel in keyframeViewModels.Skip(selectedIndex).Take(clickedIndex - selectedIndex + 1))
                        keyframeViewModel.IsSelected = true;
                }
            }
            else if (toggle)
            {
                // Toggle only the clicked keyframe, leave others alone
                clicked.IsSelected = !clicked.IsSelected;
            }
            else
            {
                // Only select the clicked keyframe
                foreach (var keyframeViewModel in keyframeViewModels)
                    keyframeViewModel.IsSelected = false;
                clicked.IsSelected = true;
            }
        }

        private List<TimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            var viewModels = new List<LayerPropertyBaseViewModel>();
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                viewModels.AddRange(layerPropertyGroupViewModel.GetAllChildren());

            var keyframes = viewModels.Where(vm => vm is LayerPropertyViewModel)
                .SelectMany(vm => ((LayerPropertyViewModel) vm).TimelinePropertyBaseViewModel.TimelineKeyframeViewModels)
                .ToList();

            return keyframes;
        }

        #endregion

        #region IViewAware

        public void AttachView(UIElement view)
        {
            if (View != null)
                throw new InvalidOperationException(string.Format("Tried to attach View {0} to ViewModel {1}, but it already has a view attached", view.GetType().Name, GetType().Name));

            View = view;
        }

        public UIElement View { get; set; }

        #endregion
    }
}