using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Icu.Text;
using Android.Widget;
using mvdata.foodjet.Validation;

namespace GCloudShared.Extensions
{
    public static class ViewExtensions
    {
        public static bool HasContent<TView>(this TView view, string errorMessage = "Das Feld darf nicht leer sein") where TView : EditText
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(view.Text))
            {
                isValid = false;
                view.Error = errorMessage;
                view.RequestFocus();
            }
            else
            {
                if (view.Error == errorMessage)
                {
                    view.Error = null;
                }
            }

            return isValid;
        }

        public static bool ContentIsEqualTo<TView>(this TView view, TView otherView, string errorMessage = "Die Werte der Felder sind nicht gleich") where TView : EditText
        {
            var isValid = true;

            if (view.Text != otherView.Text)
            {
                isValid = false;
                view.Error = errorMessage;
                otherView.Error = errorMessage;
                otherView.RequestFocus();
            }
            else
            {
                if (view.Error == errorMessage)
                {
                    view.Error = null;
                }
            }

            return isValid;
        }

        public static bool ContentMinLength<TView>(this TView view, int length, string errorMessage = "Der Text muss min. {0} Zeichen lang sein") where TView : EditText
        {
            var isValid = true;
            errorMessage = string.Format(errorMessage, length);
            if (view.Text.Length < length)
            {
                isValid = false;
                view.Error = errorMessage;
                view.RequestFocus();
            }
            else
            {
                if (view.Error == errorMessage)
                {
                    view.Error = null;
                }
            }

            return isValid;
        }
    }
}
