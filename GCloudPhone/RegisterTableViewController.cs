using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using GCloudiPhone.Extensions;
using GCloudiPhone.Helpers;
using GCloudShared.Extensions;
using GCloudShared.Service;
using GCloudShared.Service.Dto;
using GCloudShared.Service.WebShopRegisterServices;
using GCloudShared.Shared;
using Newtonsoft.Json;
using Refit;
using UIKit;

namespace GCloudiPhone
{
    public partial class RegisterTableViewController : UITableViewController, ICanCleanUpMyself
    {
        IAuthService _authService;
        UIDatePicker birthDatePicker;
        private List<NSIndexPath> expandedPaths;
        private readonly UITapGestureRecognizer tap;
        private string invitationCodeSender;
        private static Random random;

        private IWebShopService webShopService;

        private RegisterToWebShopResult registerToWebShopResult;

        public RegisterTableViewController(IntPtr handle) : base(handle)
        {
            _authService = RestService.For<IAuthService>(HttpClientContainer.Instance.HttpClient);
            webShopService = RestService.For<IWebShopService>("https://test1.willessen.online");
            tap = new UITapGestureRecognizer(DismissKeyboard);
            invitationCodeSender = null;
            random = new Random();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "DataProtectionSegue2")
            {
                var webViewController = segue.DestinationViewController as SettingsWebViewController;
                webViewController.Type = "DataProtection";
            }
            base.PrepareForSegue(segue, sender);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            expandedPaths = new List<NSIndexPath>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RegisterTable.SectionFooterHeight = 0.0f;

            //UsernameText.BecomeFirstResponder();
            EmailTextField.BecomeFirstResponder();

            birthDatePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.Date
            };
            birthDatePicker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
            birthDatePicker.SizeToFit();
            BirthDateTextField.InputView = birthDatePicker;

            var toolBar = new UIToolbar(new CGRect(0, 0, 320, 44));
            toolBar.TintColor = UIColor.FromRGB(255, 87, 34);
            var doneBtn = new UIBarButtonItem("Fertig", style: UIBarButtonItemStyle.Plain, handler: ShowSelectedDate);
            var space = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null, null);

            toolBar.SetItems(new UIBarButtonItem[] { space, doneBtn }, true);
            BirthDateTextField.InputAccessoryView = toolBar;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            View.AddGestureRecognizer(tap);

            //UsernameText.ShouldReturn = TextFieldShouldReturn;
            EmailTextField.ShouldReturn = TextFieldShouldReturn;
            PasswordTextField.ShouldReturn = TextFieldShouldReturn;
            PasswdRepeatTextField.ShouldReturn = TextFieldShouldReturn;
            //FirstNameText.ShouldReturn = TextFieldShouldReturn;
            //LastNameText.ShouldReturn = TextFieldShouldReturn;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            View.RemoveGestureRecognizer(tap);

            //UsernameText.ShouldReturn = null;
            PasswordTextField.ShouldReturn = null;
            PasswdRepeatTextField.ShouldReturn = null;
            //FirstNameText.ShouldReturn = null;
            //LastNameText.ShouldReturn = null;
            EmailTextField.ShouldReturn = null;
        }

        private void ShowSelectedDate(Object sender, EventArgs eventArgs)
        {
            var formatter = new NSDateFormatter
            {
                DateFormat = "dd. MMMM YYYY"
            };
            BirthDateTextField.Text = formatter.ToString(birthDatePicker.Date);
            BirthDateTextField.ResignFirstResponder();
        }

        public bool TextFieldShouldReturn(UITextField textField)
        {
            //if (textField == UsernameText)
            //{
            //    PasswordTextField.BecomeFirstResponder();
            //}
            if (textField == EmailTextField)
            {
                PasswordTextField.BecomeFirstResponder();
            }
            else if(textField == PasswordTextField)
            {
                PasswdRepeatTextField.BecomeFirstResponder();
            }
            else if (textField == PasswdRepeatTextField)
            {
                //FirstNameText.BecomeFirstResponder();
                BirthDateTextField.BecomeFirstResponder();
            }
            //else if(textField == FirstNameText)
            //{
            //    LastNameText.BecomeFirstResponder();
            //}
            //else if(textField == LastNameText)
            //{
            //    EmailText.BecomeFirstResponder();
            //}
            //else if(textField == EmailText)
            //{
            //    BirthDateTextField.BecomeFirstResponder();
            //}
            else if (textField == BirthDateTextField)
            {
                TxtInvitationCode.BecomeFirstResponder();
            }
            else
            {
                View.EndEditing(true);
            }

            return true;
        }

        private void DismissKeyboard()
        {
            View.EndEditing(true);
        }

        partial void RegisterButton_TouchUpInside(UIButton sender)
        {
            ToggleInput();
            PerformRegistration();
        }


        public async Task RegisterToGCloud()
        {
            DateTime dt = new DateTime(1900, 01, 01);
            var birthday = birthDatePicker.Date.ToDateTime();
            if (birthday.Date.Day == DateTime.Now.Day &&
                birthday.Date.Month == DateTime.Now.Month &&
                birthday.Date.Year == DateTime.Now.Year)
            {
                birthday = dt;
            }

            invitationCodeSender = _authService.InvitationCodeSenderId(TxtInvitationCode.Text).Result;

            await _authService.RegisterUser(new RegisterRequestModel()
            {
                ConfirmPassword = PasswdRepeatTextField.Text.Trim(),
                Username = EmailTextField.Text.Trim(),
                //Username = UsernameText.Text.Trim(),
                Password = PasswordTextField.Text.Trim(),
                Email = EmailTextField.Text.Trim(),
                //FirstName = FirstNameText.Text.Trim(),
                FirstName = "NoFirstName",
                //LastName = LastNameText.Text.Trim(),
                LastName = "NoLastName",
                Birthday = birthday,
                InvitationCode = RandomString(9),
                InvitationCodeSender = invitationCodeSender == null ? invitationCodeSender : invitationCodeSender.Replace("\"", "")
            });
        }

        public async Task RegisterToWebShop()
        {

            if (!registerToWebShopResult.Success)
            {
                await webShopService.ResetPasswordInWebShopFromGcloud(new RecoveryPasswordToWebShopModel
                {
                    Email = EmailTextField.Text.Trim(),
                    NewPassword = PasswordTextField.Text.Trim(),
                    Result = null
                });

                //mozda ispod neki alert da sam resetovala password u WebShopu na osnovu rezultata await webShopService.ResetPasswordInWebShopFromGcloud()
                //mozda i neka firebase notifikacija da se posalje....
            }
            else
            {
                await webShopService.Register(new RegisterToWebShopModel()
                {
                    ConfirmPassword = PasswdRepeatTextField.Text.Trim(),
                    //Username is email adress
                    Username = EmailTextField.Text.Trim(),
                    Password = PasswordTextField.Text.Trim(),
                    Email = EmailTextField.Text.Trim(),
                    //No First and Last Name
                    FirstName = "No name",
                    LastName = "No name"

                });

                await webShopService.SetWelcomeEmailToWebShopFromGcloud(new RecoveryPasswordToWebShopModel
                {
                    Email = EmailTextField.Text.Trim(),
                    NewPassword = PasswordTextField.Text.Trim(),
                    Result = null

                });
            }


        }




        private async void PerformRegistration()
        {



            try
            {
                if (!IsValid())
                {
                    ToggleInput();
                    return;
                }

                //bool isInvitationCodeValid = _authService.IsInvitationCodeAvailable(TxtInvitationCode.Text).Result;
                //if (!isInvitationCodeValid)
                //{
                //    ToggleInput();
                //    AddError(NSIndexPath.FromRowSection(4, 1), "Freundes-Code ist ungültig");
                //    return;
                //}

                //invitationCodeSender = _authService.InvitationCodeSenderId(TxtInvitationCode.Text).Result;

                //// If user leaves birthday field empty, set dt as his birthday (default value)
                //DateTime dt = new DateTime(1900, 01, 01);
                //var birthday = birthDatePicker.Date.ToDateTime();
                //if (birthday.Date.Day == DateTime.Now.Day && 
                //    birthday.Date.Month == DateTime.Now.Month && 
                //    birthday.Date.Year == DateTime.Now.Year)
                //{
                //    birthday = dt;
                //}

                //await _authService.RegisterUser(new RegisterRequestModel()
                //{
                //    ConfirmPassword = PasswdRepeatTextField.Text.Trim(),
                //    Username = EmailTextField.Text.Trim(),
                //    //Username = UsernameText.Text.Trim(),
                //    Password = PasswordTextField.Text.Trim(),
                //    Email = EmailTextField.Text.Trim(),
                //    //FirstName = FirstNameText.Text.Trim(),
                //    FirstName = "NoFirstName",
                //    //LastName = LastNameText.Text.Trim(),
                //    LastName = "NoLastName",
                //    Birthday = birthday,
                //    InvitationCode = RandomString(9),
                //    InvitationCodeSender = invitationCodeSender == null ? invitationCodeSender : invitationCodeSender.Replace("\"","")
                //});


                registerToWebShopResult = JsonConvert.DeserializeObject<RegisterToWebShopResult>(webShopService.CheckIfUserIsAlreadyRegistredInWebShop(new RegisterToWebShopModel()
                {
                    ConfirmPassword = PasswdRepeatTextField.Text.Trim(),
                    //Username is email adress
                    Username = EmailTextField.Text.Trim(),
                    Password = PasswordTextField.Text.Trim(),
                    Email = EmailTextField.Text.Trim(),
                    //No First and Last Name
                    FirstName = "No name",
                    LastName = "No name"
                }).Result);


                await Task.WhenAny(RegisterToGCloud(), RegisterToWebShop());

                InvokeOnMainThread(() =>
                {
                    var alert = UIAlertController.Create("Registrierung erfolgreich", "Registrierung war erfolgreich. Du wirst in kürze eine E-Mail erhalten, um deinen Account zu aktivieren. Kontrolliere bitte auch deinen Junk-E-Mail-/Spam-Ordner.", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action =>
                    {
                        NavigationController.PopViewController(true);
                        alert.Dispose();
                    }));
                    PresentViewController(alert, true, null);
                    ToggleInput();
                });
            }
            catch (ApiException apiException)
            {
                InvokeOnMainThread(() =>
                {
                    var errorModel = apiException.GetApiErrorResult();
                    errorModel.Match(some =>
                    {
                        RegisterTable.TableHeaderView = new UILabel(new CGRect(16, 16, View.Bounds.Width - 32, 21))
                        {
                            Text = some.Message,
                            Font = UIFont.PreferredBody,
                            TextColor = UIColor.Red
                        };
                    }, () =>
                    {
                        var alert = UIAlertController.Create("Registrierung fehlgeschlagen", "Es ist ein unbekannter Fehler aufgetreten.", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, action =>
                        {
                            ToggleInput();
                            alert.Dispose();
                        }));
                        PresentViewController(alert, true, null);
                    });
                    ToggleInput();
                });
            }
        }

        private bool IsValid()
        {
            var isValid = true;
            RegisterTable.BeginUpdates();

            //if (string.IsNullOrWhiteSpace(UsernameText.Text.Trim()))
            //{
            //    isValid = false;
            //    AddError(NSIndexPath.FromRowSection(0, 0), "Benutzername darf nicht leer sein");
            //}

            if (string.IsNullOrWhiteSpace(EmailTextField.Text.Trim()))
            {
                isValid = false;
                AddError(NSIndexPath.FromRowSection(1, 0), "E-Mail darf nicht leer sein");
            }

            if (string.IsNullOrWhiteSpace(PasswordTextField.Text.Trim()))
            {
                isValid = false;
                AddError(NSIndexPath.FromRowSection(2, 0), "Passwort darf nicht leer sein");
            }

            if (string.IsNullOrWhiteSpace(PasswdRepeatTextField.Text.Trim()))
            {
                isValid = false;
                AddError(NSIndexPath.FromRowSection(3, 0), "Passwortwiederholung darf nicht leer sein");
            }

            //if (string.IsNullOrWhiteSpace(FirstNameText.Text.Trim()))
            //{
            //    isValid = false;
            //    AddError(NSIndexPath.FromRowSection(0, 1), "Vorname darf nicht leer sein");
            //}

            //if (string.IsNullOrWhiteSpace(LastNameText.Text.Trim()))
            //{
            //    isValid = false;
            //    AddError(NSIndexPath.FromRowSection(1, 1), "Nachname darf nicht leer sein");
            //}


            //if (string.IsNullOrWhiteSpace(BirthDateTextField.Text.Trim()))
            //{
            //    isValid = false;
            //    AddError(NSIndexPath.FromRowSection(3, 1), "Geburtsdatum darf nicht leer sein");
            //}

            //if (string.IsNullOrWhiteSpace(TxtInvitationCode.Text.Trim()))
            //{
            //    isValid = false;
            //    AddError(NSIndexPath.FromRowSection(4, 1), "Freundes-Code darf nicht leer sein");
            //}

            //if a user, who sent invitation code, has valid invitation code, then false is returned
            //TODO: Fix naming convention (it should return true by logic)
            bool isInvitationCodeValid = _authService.IsInvitationCodeAvailable(TxtInvitationCode.Text).Result;

            if (isInvitationCodeValid)
            {
                isValid = false;
                AddError(NSIndexPath.FromRowSection(1, 1), "Freundes-Code ist ungültig");
            }

            RegisterTable.EndUpdates();
            return isValid;
        }

        private void AddError(NSIndexPath indexPath, string errorMessage)
        {
            var cell = RegisterTable.CellAt(indexPath);
            if (cell != null && !expandedPaths.Contains(indexPath))
            {
                // var errorLabel = new UILabel(new CGRect(cell.Bounds.X + 16, cell.Bounds.Y + UsernameText.Bounds.Height + 2, cell.Bounds.Width + 32, 21))
                var errorLabel = new UILabel(new CGRect(cell.Bounds.X + 16, cell.Bounds.Y + PasswordTextField.Bounds.Height + 2, cell.Bounds.Width + 32, 21))
                {
                    Text = errorMessage,
                    Font = UIFont.PreferredBody,
                    TextColor = UIColor.Red
                };
                errorLabel.SizeToFit();
                cell.AddSubview(errorLabel);
                expandedPaths.Add(indexPath);
            }
        }


        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return expandedPaths.Contains(indexPath) ? 75 : (nfloat)44;
        }

        private void ToggleInput()
        {
            //UsernameText.Enabled = !UsernameText.Enabled;
            //FirstNameText.Enabled = !FirstNameText.Enabled;
            //LastNameText.Enabled = !LastNameText.Enabled;
            EmailTextField.Enabled = !EmailTextField.Enabled;
            PasswordTextField.Enabled = !PasswordTextField.Enabled;
            PasswdRepeatTextField.Enabled = !PasswdRepeatTextField.Enabled;
            LoadingIndicator.Hidden = !LoadingIndicator.Hidden;
            RegisterButton.Hidden = !RegisterButton.Hidden;
        }

        public void CleanUp()
        {
            _authService = null;
            birthDatePicker.Dispose();
            birthDatePicker = null;
            expandedPaths.ForEach(e => e.Dispose());
            expandedPaths = null;

            ReleaseDesignerOutlets();
        }

        public static String RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}