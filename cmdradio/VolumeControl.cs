using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace cmdradio
{
    public class VolumeControl : ProgressBar
    {
        public VolumeControl( ) {
            AddHandler( MouseMoveEvent, new MouseEventHandler(onMouseMove) );
            AddHandler(MouseDownEvent, new MouseEventHandler(onMouseDown));
            AddHandler( MouseUpEvent, new MouseEventHandler(onMouseUp) );
        }

        private bool capturing = false;

        private void onMouseDown(object sender, MouseEventArgs e) {
            if ( e.LeftButton == MouseButtonState.Pressed ) {
                ConsoleApplication.Instance.BeginCaptureInput(this);
                capturing = true;
                Percent = ( int ) ( 100*(e.GetPosition( this ).X + 1)*1.0/this.ActualWidth );
                Invalidate(  );
                e.Handled = true;
            }
        }

        private void onMouseUp( object sender, MouseEventArgs e ) {
            if ( capturing && e.LeftButton == MouseButtonState.Released ) {
                ConsoleApplication.Instance.EndCaptureInput( this );
                capturing = false;
            }
        }

        private void onMouseMove( object sender, MouseEventArgs e ) {
            if ( capturing ) {
                int percent = ( int ) ( 100*(e.GetPosition( this ).X + 1)*1.0/this.ActualWidth );
                Percent = percent < 0 ? 0 : ( percent > 100 ? 100 : percent );
                Invalidate(  );
                e.Handled = true;
            }
        }
    }
}
