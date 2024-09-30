using IRC.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IRC.Models.Message;

namespace IRC
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageType messageType)
            {
                return messageType switch
                {
                    MessageType.UserSent => Colors.Blue,
                    MessageType.Received => Colors.White,
                    MessageType.Warning => Colors.Orange,
                    MessageType.Error => Colors.Red,
                    MessageType.System => Colors.Gray,
                    _ => Colors.HotPink,
                };
            }
            return Colors.HotPink;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
