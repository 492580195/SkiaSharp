﻿using System;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaSharpSample
{
	public abstract class SampleBase
	{
		protected SKMatrix Matrix = SKMatrix.MakeIdentity();

		private SKMatrix startPanMatrix = SKMatrix.MakeIdentity();
		private SKMatrix startPinchMatrix = SKMatrix.MakeIdentity();
		private SKPoint startPinchOrigin = SKPoint.Empty;
		private float totalPinchScale = 1f;

		public abstract string Title { get; }

		public virtual string Description { get; } = string.Empty;

		public virtual SamplePlatforms SupportedPlatform { get; } = SamplePlatforms.All;

		public virtual SampleBackends SupportedBackends { get; } = SampleBackends.All;

		public virtual SampleCategories Category { get; } = SampleCategories.General;

		public bool IsInitialized { get; private set; } = false;

		public void DrawSample(SKCanvas canvas, int width, int height)
		{
			if (IsInitialized)
			{
				canvas.SetMatrix(Matrix);
				OnDrawSample(canvas, width, height);
			}
		}

		protected abstract void OnDrawSample(SKCanvas canvas, int width, int height);

		public async void Init(Action callback = null)
		{
			// reset the matrix for the new sample
			Matrix = SKMatrix.MakeIdentity();

			if (!IsInitialized)
			{
				await OnInit();

				IsInitialized = true;

				callback?.Invoke();
			}
		}

		protected virtual Task OnInit()
		{
			return Task.FromResult(true);
		}

		public void Tap()
		{
			if (IsInitialized)
			{
				OnTapped();
			}
		}

		protected virtual void OnTapped()
		{
		}

		public void Pan(GestureState state, SKPoint translation)
		{
			switch (state)
			{
				case GestureState.Started:
					startPanMatrix = Matrix;
					break;
				case GestureState.Running:
					var canvasTranslation = SKMatrix.MakeTranslation(translation.X, translation.Y);
					SKMatrix.Concat(ref Matrix, ref canvasTranslation, ref startPanMatrix);
					break;
				default:
					startPanMatrix = SKMatrix.MakeIdentity();
					break;
			}
		}

		public void Pinch(GestureState state, float scale, SKPoint origin)
		{
			switch (state)
			{
				case GestureState.Started:
					startPinchMatrix = Matrix;
					startPinchOrigin = origin;
					totalPinchScale = 1f;
					break;
				case GestureState.Running:
					totalPinchScale *= scale;
					var pinchTranslation = origin - startPinchOrigin;
					var canvasTranslation = SKMatrix.MakeTranslation(pinchTranslation.X, pinchTranslation.Y);
					var canvasScaling = SKMatrix.MakeScale(totalPinchScale, totalPinchScale, origin.X, origin.Y);
					var canvasCombined = SKMatrix.MakeIdentity();
					SKMatrix.Concat(ref canvasCombined, ref canvasScaling, ref canvasTranslation);
					SKMatrix.Concat(ref Matrix, ref canvasCombined, ref startPinchMatrix);
					break;
				default:
					startPinchMatrix = SKMatrix.MakeIdentity();
					startPinchOrigin = SKPoint.Empty;
					totalPinchScale = 1f;
					break;
			}
		}

		public virtual bool MatchesFilter(string searchText)
		{
			if (string.IsNullOrWhiteSpace(searchText))
				return true;
			
			return
				Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1 ||
				Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}

	public enum GestureState
	{
		Started,
		Running,
		Completed,
		Canceled
	}
}
