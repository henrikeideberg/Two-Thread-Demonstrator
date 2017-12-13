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
		int angle = 0;
		private delegate void DisplayDelegate(int x, int y, string text);
		private DisplayDelegate delDisplay;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void buttonStartDisplayinh_Click(object sender, RoutedEventArgs e)
		{
			//CreateTextBlockMainThread("ThreadName");

			Thread newThread = new Thread(CreateTextBlockOtherThread);
			newThread.SetApartmentState(ApartmentState.STA);
			newThread.Start("Henrik");
		}

		private void buttonStartRotate_Click(object sender, RoutedEventArgs e)
		{
			CreateRotatedPolygon(this.angle);
			this.angle += 5;
		}

		private void CreateTextBlockOtherThread(object name)
		{
			Random rnd = new Random();
			int y;
			int x;
			string threadName = Convert.ToString(name);
			int i = 0;
			while(i < 5)
			{
				y = rnd.Next(300);
				x = rnd.Next(300);

				/* 
				 * Below commented out code causes exception at textBlock because
				 * textblock is not created in main thread.
				 */
				//Application.Current.Dispatcher.BeginInvoke(
				//  DispatcherPriority.Background,
				//  new Action(() => this.canvasDisplayThreadName.Children.Add(textBlock)));
				delDisplay = new DisplayDelegate(UpdateCanvasDisplayThreadName);
				Dispatcher.BeginInvoke(delDisplay, new object[] { x, y, threadName });
				Thread.Sleep(1000);
				i++;
			}

		}

		private void UpdateCanvasDisplayThreadName(int x, int y, string text)
		{
			this.canvasDisplayThreadName.Children.Clear();

			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;// "Thread Fucking Name"; //Convert.ToString(name);

			textBlock.Foreground = new SolidColorBrush(Colors.Black);

			Canvas.SetLeft(textBlock, x);
			Canvas.SetTop(textBlock, y);
			this.canvasDisplayThreadName.Children.Add(textBlock);
		}

		private void CreateTextBlockMainThread(string name)
		{
			canvasDisplayThreadName.Children.Clear();

			Random rnd = new Random();
			int y = rnd.Next((int)canvasDisplayThreadName.Height);
			int x = rnd.Next((int)canvasDisplayThreadName.Width);

			TextBlock textBlock = new TextBlock();
			textBlock.Text = name;

			textBlock.Foreground = new SolidColorBrush(Colors.Black);

			Canvas.SetLeft(textBlock, x);
			Canvas.SetTop(textBlock, y);

			canvasDisplayThreadName.Children.Add(textBlock);
		}

		/// <summary>
		/// //https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/shapes-and-basic-drawing-in-wpf-overview
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
			myPolygon.Fill = Brushes.Blue;
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
	}
}
