using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce
{
    interface IMarketPlace
    {
        bool ValidateCredentials(string shopUrl, string consumerKey, string consumerSecret);

        void SyncOrders(string shopUrl, string consumerKey, string consumerSecret);

        bool FulfillOrders(string shopUrl, string consumerKey, string consumerSecret, int orderId);
    }
}
