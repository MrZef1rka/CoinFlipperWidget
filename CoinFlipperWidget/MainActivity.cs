using System;
using System.Threading.Tasks;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace CoinFlipperWidget
{
    [BroadcastReceiver(Label = "Coin Flipper Widget")]
    [IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData(AppWidgetManager.MetaDataAppwidgetProvider, Resource = "@xml/appwidget_info")]
    public class CoinFlipperWidget : AppWidgetProvider
    {
        private static readonly Random Random = new Random();
        private static bool _isAnimating;

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            // Update all app widgets
            foreach (var appWidgetId in appWidgetIds)
            {
                // Create a new RemoteViews object for the widget layout
                var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);

                // Set a click event on the widget
                remoteViews.SetOnClickPendingIntent(Resource.Id.widget_layout, GetPendingSelfIntent(context, appWidgetId, AppWidgetManager.ActionAppwidgetUpdate));

                // Update the widget with the new layout
                appWidgetManager.UpdateAppWidget(appWidgetId, remoteViews);
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Handle the app widget update event
            if (AppWidgetManager.ActionAppwidgetUpdate.Equals(intent.Action))
            {
                var appWidgetId = intent.GetIntExtra(AppWidgetManager.ExtraAppwidgetId, AppWidgetManager.InvalidAppwidgetId);

                // Check if the widget is already animating and if the app widget ID is valid
                if (!_isAnimating && appWidgetId != AppWidgetManager.InvalidAppwidgetId)
                {
                    // Set the widget as animating
                    _isAnimating = true;

                    // Animate the coin flip
                    AnimateCoinFlip(context, appWidgetId);
                }
            }
        }

        private static async void AnimateCoinFlip(Context context, int appWidgetId)
        {
            // Create a new RemoteViews object for the widget layout
            var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);

            // Set the coin image to the default coin image
            remoteViews.SetImageViewResource(Resource.Id.widget_coin, Resource.Drawable.coin);

            // Disable click events on the widget during the animation
            remoteViews.SetOnClickPendingIntent(Resource.Id.widget_layout, null);

            // Update the widget with the new layout
            AppWidgetManager.GetInstance(context).UpdateAppWidget(appWidgetId, remoteViews);

            // Wait for 1 second
            await Task.Delay(1000);

            // Create a new instance of the random number generator with a seed value
            var random = new Random(Guid.NewGuid().GetHashCode());

            // Generate a random result for the coin flip
            var result = random.Next(2) == 0 ? Resource.Drawable.heads : Resource.Drawable.tails;

            // Set the coin image to the result of the coin flip
            remoteViews.SetImageViewResource(Resource.Id.widget_coin, result);

            // Update the widget with the new layout
            AppWidgetManager.GetInstance(context).UpdateAppWidget(appWidgetId, remoteViews);

            // Set the widget as not animating
            _isAnimating = false;

            // Set the click event back on the widget
            var pendingIntent = GetPendingSelfIntent(context, appWidgetId, AppWidgetManager.ActionAppwidgetUpdate);
            remoteViews.SetOnClickPendingIntent(Resource.Id.widget_layout, pendingIntent);

            // Update the widget with the new layout
            AppWidgetManager.GetInstance(context).UpdateAppWidget(appWidgetId, remoteViews);
        }

        private static PendingIntent GetPendingSelfIntent(Context context, int appWidgetId, string action)
        {
            // Create a new intent for the app widget provider
            var intent = new Intent(context, typeof(CoinFlipperWidget));

            // Set the action and app widget ID extras
            intent.SetAction(action);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetId, appWidgetId);

            // Get a broadcast PendingIntent for the app widget provider
            return PendingIntent.GetBroadcast(context, appWidgetId, intent, 0);
        }
    }
}
