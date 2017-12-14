using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Two_Thread_Demonstrator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private delegate void DisplayThreadNameDelegate(int x, int y, string text); //Delegate for invoking gui updates in main thread
		private delegate void DisplayRotatedTriangleDelegate(int angle); //Delegate for invoking gui updates in main thread
		private DisplayThreadNameDelegate delDisplayThreadName;
		private DisplayRotatedTriangleDelegate delDisplayRotatedTriangle;
		private Thread displayNameThread;
		private Thread rotateTriangle;
		private int maxThreadIterations = 500;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Method which run when buttonStartDisplaying is clicked. 
		/// This method will start GetDisplayThreadNamePropertiesFromOtherThread on a thread,
		/// (different than main thread).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonStartDisplaying_Click(object sender, RoutedEventArgs e)
		{
			displayNameThread = new Thread(GetDisplayThreadNamePropertiesFromOtherThread);
			displayNameThread.Start("Henrik");
		}

		/// <summary>
		/// Method which run when buttonStartRotate is clicked.
		/// This method will start CreateRotatedTriangleOtherThread on a thread,
		/// (different than main thread).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonStartRotate_Click(object sender, RoutedEventArgs e)
		{
			rotateTriangle = new Thread(CreateRotatedTriangleOtherThread);
			rotateTriangle.Start();
		}

		/// <summary>
		/// Method to set the properties of a textblock.
		/// Properties set are;
		///  - text to disply/show
		///  - x and y coordinates for the textblock
		/// </summary>
		/// <param name="name"></param>
		private void GetDisplayThreadNamePropertiesFromOtherThread(object name)
		{
			Random rnd = new Random();
			int y;
			int x;
			string threadName = Convert.ToString(name);
			int i = 0;
			while(i < maxThreadIterations)
			{
				y = rnd.Next(300);
				x = rnd.Next(300);

				/* 
				 * If I create textblock in thish thread, then 
				 * below code causes exception at textBlock because
				 * textblock is not created in main thread.
				 */
				//Application.Current.Dispatcher.BeginInvoke(
				//  DispatcherPriority.Background,
				//  new Action(() => this.canvasDisplayThreadName.Children.Add(textBlock)));

				/*
				 * Use the display delegate to draw on the canvas from main thread.
				 * From my understanding we cannot pass GUI elements between threads,
				 * hence this thread creates the properties of the textblock and
				 * pass them to the main thread (via the delegate).
				 * The textblock is then created by the main thread 
				 * (with the data/properties from this thread) and drawn on the canvas.
				 */
				delDisplayThreadName = new DisplayThreadNameDelegate(UpdateCanvasDisplayThreadName);
				Dispatcher.BeginInvoke(delDisplayThreadName, new object[] { x, y, threadName });

				//Sleep the thread for one second and then step i
				Thread.Sleep(1000);
				i++;
			}

		}

		/// <summary>
		/// Method to set rotation angle of triangle
		/// </summary>
		private void CreateRotatedTriangleOtherThread()
		{
			int i = 0;
			int angle = 0;
			while(i < maxThreadIterations)
			{
				//Use display delegate to draw rotated triangle on canvas from main thread
				delDisplayRotatedTriangle = new DisplayRotatedTriangleDelegate(CreateRotatedPolygon);
				Dispatcher.BeginInvoke(delDisplayRotatedTriangle, new object[] { angle });

				//Sleep thread for one second and then step i and angle.
				Thread.Sleep(1000);
				i++;
				angle = (angle + 5) % 360;
			}
		}

		/// <summary>
		/// Method that
		///  - runs on main thread
		///  - clear the canvas canvasDisplayThreadName of all children
		///  - add a textblock to same canvas
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="text"></param>
		private void UpdateCanvasDisplayThreadName(int x, int y, string text)
		{
			this.canvasDisplayThreadName.Children.Clear();
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.Foreground = new SolidColorBrush(Colors.Black);
			Canvas.SetLeft(textBlock, x);
			Canvas.SetTop(textBlock, y);
			this.canvasDisplayThreadName.Children.Add(textBlock);
		}

		/// <summary>
		/// Method that
		///  - runs on main thread
		///  - clears the canvas canvasRotateTriangle of all children
		///  - adds a polygon (in the shape of a triangle) to same canavs 
		/// https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/shapes-and-basic-drawing-in-wpf-overview
		/// </summary>
		/// <param name="angle"></param>
		private void CreateRotatedPolygon(int angle)
		{
			canvasRotateTriangle.Children.Clear();
			PointCollection myPointCollection = new PointCollection();
			myPointCollection.Add(new Point(0, 0));
			myPointCollection.Add(new Point(0, 1));
			myPointCollection.Add(new Point(1, 1));
			Polygon myPolygon = new Polygon();
			myPolygon.Points = myPointCollection;
			myPolygon.Width = 100;
			myPolygon.Height = 100;
			myPolygon.Stretch = Stretch.Fill;
			myPolygon.Stroke = Brushes.Black;
			myPolygon.StrokeThickness = 2;
			double left = (canvasRotateTriangle.ActualWidth - 100) / 2;
			Canvas.SetLeft(myPolygon, left);
			double top = (canvasRotateTriangle.ActualHeight - 100) / 2;
			Canvas.SetTop(myPolygon, top);
			RotateTransform rotateTransform1 = new RotateTransform(angle, 0, 0);
			myPolygon.RenderTransform = rotateTransform1;
			canvasRotateTriangle.Children.Add(myPolygon);
		}

		/// <summary>
		/// Method which runs when button buttonStopDisplaying is clicked.
		/// Will abort the thread displayNameThread if running.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonStopDisplaying_Click(object sender, RoutedEventArgs e)
		{
			if (displayNameThread != null)
			{
				if (displayNameThread.IsAlive) displayNameThread.Abort();
			}
		}

		/// <summary>
		/// Method which runs when button buttonStopRotate is clicked.
		/// Will abort the thread rotateTriangle if running.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonStopRotate_Click(object sender, RoutedEventArgs e)
		{
			if(rotateTriangle != null)
			{
				if (rotateTriangle.IsAlive) rotateTriangle.Abort();
			}
		}

		/// <summary>
		/// Method which runs when application is closing.
		/// Will abort the threads rotateTriangle and displayNameThread if running
		/// and display a "Bye" message box.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckThreads(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (rotateTriangle != null)
			{
				if (rotateTriangle.IsAlive) rotateTriangle.Abort();
			}
			if (displayNameThread != null)
			{
				if (displayNameThread.IsAlive) displayNameThread.Abort();
			}
			MessageBoxResult result = MessageBox.Show("Bye");
		}
	}
}
