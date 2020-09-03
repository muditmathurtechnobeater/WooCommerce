using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using ShippingLine = WooCommerceNET.WooCommerce.v2.OrderShippingLine;
using OrderMeta = WooCommerceNET.WooCommerce.v2.OrderMeta;

namespace WooCommerce
{
    public class WooCommerce : IMarketPlace
    {
        /// <summary>
        /// Validate the credentials 
        /// </summary>
        /// <param name="shopUrl"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <returns></returns>
        public bool ValidateCredentials(string shopUrl, string consumerKey, string consumerSecret)
        {
            bool isValidated = false;
            try
            {
                var restAPI = ApiClient(shopUrl, consumerKey, consumerSecret);
                var products = restAPI.SendHttpClientRequest<Product>("products", RequestMethod.GET, null, null).Result;
                if (products != null)
                {
                    var productResponse = JsonConvert.DeserializeObject<Product>(products);
                    isValidated = true;
                }
            }
            catch (Exception ex)
            {
                isValidated = false;
            }
            return isValidated;
        }


        /// <summary>
        /// Sync the orders 
        /// </summary>
        /// <param name="shopUrl"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        public void SyncOrders(string shopUrl, string consumerKey, string consumerSecret)
        {
            var restAPI = ApiClient(shopUrl, consumerKey, consumerSecret);

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", "processing");

            var response = restAPI.SendHttpClientRequest<IList<Order>>("orders", RequestMethod.GET, null, param).Result;

            if (response != null)
            {
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);
            }
        }

        /// <summary>
        /// Fuilfill Order 
        /// </summary>
        /// <param name="shopUrl"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool FulfillOrders(string shopUrl, string consumerKey, string consumerSecret, int orderId)
        {
            var restAPI = ApiClient(shopUrl, consumerKey, consumerSecret);
            bool isFulfilled = false;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("status", "completed");

            var response = restAPI.SendHttpClientRequest<Order>("orders/" + orderId, RequestMethod.PUT, null, param).Result;
            if (response != null)
            {
                UpdateOrderMetaData(orderId, shopUrl, consumerKey, consumerSecret);
                var orderResponse = JsonConvert.DeserializeObject<Order>(response);
                if (orderResponse != null && orderResponse.status == "completed")
                {
                    isFulfilled = true;
                }
            }
            return isFulfilled;
        }


        /// <summary>
        /// Update the order metadata
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="marketPlaceConfiguration"></param>
        /// <returns></returns>
        private bool UpdateOrderMetaData(int orderId, string shopUrl, string consumerKey, string consumerSecret)
        {
            bool isOrderMetadataUpdated = false;
            try
            {
                var restAPI = ApiClient(shopUrl, consumerKey, consumerSecret);

                Order request = new Order();
                request.id = Convert.ToInt32(orderId);
                request.shipping_lines = new List<ShippingLine>();

                var shippingLine = new ShippingLine();

                shippingLine.method_id = request.id.ToString();
                shippingLine.method_title = "UPS Standard";
                shippingLine.total = 100;
                request.shipping_lines.Add(shippingLine);

                request.meta_data = new List<OrderMeta>();

                var orderItem1 = new OrderMeta();
                orderItem1.id = request.id;
                orderItem1.key = "Tracking Provider";
                orderItem1.value = "UPS Standard";

                var orderItem2 = new OrderMeta();
                orderItem2.id = request.id;
                orderItem2.key = "Master Tracking Number";
                orderItem2.value = "1Z23456489789";

                request.meta_data.Add(orderItem1);
                request.meta_data.Add(orderItem2);


                var response = restAPI.SendHttpClientRequest<Order>("orders/" + orderId, RequestMethod.PUT, request, null).Result;
                if (response != null)
                {
                    var orderResponse = JsonConvert.DeserializeObject<Order>(response);
                    if (orderResponse != null && orderResponse.status == "completed")
                        isOrderMetadataUpdated = true;
                    else
                        isOrderMetadataUpdated = false;
                }
            }
            catch (Exception ex)
            {
                isOrderMetadataUpdated = false;
            }
            return isOrderMetadataUpdated;
        }

        /// <summary>
        /// Rest API Client Method to call the request 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        private static RestAPI ApiClient(string url, string key, string secret)
        {
            try
            {
                RestAPI rest = new RestAPI(url + "/wp-json/wc/v3", key, secret);
                return rest;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
