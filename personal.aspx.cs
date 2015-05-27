using Infragistics.WebUI.UltraWebGrid;
using Meridian.BL;
using MeridianEHealth.Library;
using MeridianEHealth.Sponsor;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.UI.WebControls;

using com.sipstorm.partners;
using VBOS.SipService;

namespace MeridianEHealth
{
    public partial class Personal : BasePage
    {

        E164 objFinancePhone;
        E164 objHomePhone;
        E164 objWorkPhone;
        E164 objAssignedNumber;
        E164 objCellPhone;
        E164 objWorkCellPhone;

        Nullable<DateTime> dtTheBirthDate;
        CreditCardValidation objCreditCardValidation;
        String strCreditError;
        Boolean isValidCreditCard;
        Decimal Balance = 0;
        private String VS_BALANCE = "BALANCE";
        private Decimal m_dBalance;
        BrandBL objBrand = BrandBL.getBrand();
        public Decimal dBalance
        {
            get
            {
                if (Decimal.TryParse(Convert.ToString(ViewState[VS_BALANCE]), out m_dBalance))
                    return m_dBalance;
                return 0;
            }
            set { ViewState[VS_BALANCE] = value; }
        }

        #region "service url"
        private readonly static string ExistsUser = ConfigurationManager.AppSettings[Constant.PROVISION_API] == "Production" ? "http://chat.sipstorm.net:9090/plugins/ssprovisioning/provision?action=jid_exists&username={0}&authuser=admin&authpassword=s1pst0rm" :
            "http://agents.dev.sipstorm.net:9190/plugins/ssprovisioning/provision?action=jid_exists&username={0}&authuser=admin&authpassword=s1pst0rm";
        private readonly static string SetPassword = ConfigurationManager.AppSettings[Constant.PROVISION_API] == "Production" ? "http://chat.sipstorm.net:9090/plugins/ssprovisioning/provision?action=set_password&username={0}&password={1}&authuser=admin&authpassword=s1pst0rm" :
        "http://agents.dev.sipstorm.net:9190/plugins/ssprovisioning/provision?action=set_password&username={0}&password={1}&authuser=admin&authpassword=s1pst0rm";
        #endregion "service url"

        #region "Events"

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.Page_Load()");
                Master.strPageTitle = Constant.MY_ACCOUNT;
                //Master.SetMenuSelected = Constant.SECURITY_USER;
                //Master.SetSelectedNode = Constant.TREE_NODE_MY_ACCOUNT;
                Master.OkClick += new EventHandler(Master_OkClick);
                Master.NoClick += new EventHandler(Master_NoClick);
                Master.strSubModule = Constant.SECURITY_USER;
                setColor();

                if (!IsPostBack)
                {
                    securityCheck.checkAccess(Constant.SECURITY_USER, false);
                    Master.SetDetails = Utils.getScreenText(securityCheck.iMemberId, Constant.SCOPE_PERSONAL_INFORMATION);
                    if (SessionInfo.iSelectedTab.SetInSession && SessionInfo.iSelectedTab.id.HasValue)
                    {
                        UltraWebTab1.SelectedTab = SessionInfo.iSelectedTab.id.Value;
                        if (SessionInfo.iSelectedTab.id.Value == 3 && SessionInfo.GridIndexID.SetInSession)
                        {
                            gvChargePayment.DisplayLayout.Pager.CurrentPageIndex = SessionInfo.GridIndexID.id.Value;
                        }

                    }
                    LoadLanguagesDropdownList();
                    SessionInfo.iSelectedTab.Unset();
                    BindProduct();
                    BindUsageHistory();
                    BindMyNumbers();
                    configureTab();
                    configurePayment(securityCheck.checkScope(Constant.SCOPE_CHARGE_PAYMENT));
                    configureSecurtiy(securityCheck.checkScope(Constant.SCOPE_SECURITY));
                    configureFinance(securityCheck.checkScope(Constant.SCOPE_FINANCIAL));
                    configureUsage(securityCheck.checkScope(Constant.SCOPE_USAGE_HISTORY));
                    confirureMyNumbers(securityCheck.checkScope(Constant.SCOPE_MY_NUMBERS));
                    confirureMyNumbers(securityCheck.checkScope(Constant.SCOPE_PRODUCTS));
                    configureHistory();
                    loadStates();
                    loadStatus();
                    loadCondition();
                    loadCreditCard();
                    ShowHidePlanType();
                    fillPersonalInfo();
                    checkpreviousPage();
                    //disableRequiredField();
                    //LoadPaymentAndChargeData();
                    securityCheck.checkScope(Constant.SCOPE_PERSONAL_INFORMATION);
                    chkSecurityQst.Attributes.Add("onClick", "CheckSecurityQst();");
                }
                if (chkSecurityQst.Checked)
                {
                    trSecurityQst.Style.Add("visibility", "visible");
                    rfvAdditionalAnswer.Enabled = true;
                    rfvAdditionalQuestion.Enabled = true;
                }
                else
                {
                    trSecurityQst.Style.Add("visibility", "collapse");
                    rfvAdditionalAnswer.Enabled = false;
                    rfvAdditionalQuestion.Enabled = false;
                }

                AppLogger.logInfo("End: MeridianEHealth.Personal.Page_Load()");
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.FETCH_FAILED;
            }
        }

        protected void Master_OkClick(object sender, EventArgs e)
        {
            try
            {
                MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
                thisMember.iActive = 1;
                thisMember.bIsDependent = false;
                thisMember.Save();
                MemberFinanceBL thisMemberfinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
                if (thisMemberfinance == null)
                    thisMemberfinance = new MemberFinanceBL();
                thisMemberfinance.iMemberId = securityCheck.iMemberId;
                thisMemberfinance.iMembershipDesired = Convert.ToInt32(txtMemberShipDesired.Text);
                Int32 _iPlanType;
                if (Int32.TryParse(ddlPlans.SelectedValue, out _iPlanType))
                    thisMemberfinance.iPlanType = _iPlanType;
                if (thisMember != null)
                {
                    if (thisMember.iSponsorProgramId.HasValue)
                    {
                        SponsorProgramBL thisSponsorProgram = SponsorProgramBL.getDataById(thisMember.iSponsorProgramId.Value);
                        if (thisSponsorProgram != null && thisSponsorProgram.iPayBy.HasValue)
                        {
                            if (thisSponsorProgram.iPayBy.Value == Convert.ToInt32(PAIDBY.INDIVIDUAL))
                            {
                                thisMemberfinance = saveCreditCardInfo(thisMemberfinance);
                            }
                        }
                    }
                    else
                    {
                        thisMemberfinance = saveCreditCardInfo(thisMemberfinance);
                    }
                }
                thisMemberfinance.Save();
                Response.Redirect(@"~\login.aspx");
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void Master_NoClick(object sender, EventArgs e)
        {
        }


        protected void lnkUpdateContact_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.lnkUpdateContact_Click()");
                if (!validateCellPhoneCapability())
                {
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateContact_Click()");
                    return;
                }
                if (validateHeader())
                {
                    MembersBL thisMember = getMember();
                    thisMember.Save();
                    Master.MessageTitle = Constant.SUCCESS;
                    Master.Message = Constant.SAVE_SUCCESS + strCreditError;
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateContact_Click()");
                    return;
                }
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void lnkUpdateMember_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.lnkUpdateMember_Click()");
                assignValidationGroup(Constant.VALIDATONGRP_SECURITY);
                if (validateHeader())
                {
                    MembersBL thisMember = getMember();
                    thisMember.Save();
                    Master.MessageTitle = Constant.SUCCESS;
                    Master.Message = Constant.SAVE_SUCCESS;
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateMember_Click()");
                    return;
                }
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }


        protected void lnkSeeDetails_Click(object sender, EventArgs e)
        {
            Int32 TransctionType, iHistoryId;
            Boolean bIsOneTime;
            SessionInfo.GridIndexID.id = gvChargePayment.DisplayLayout.Pager.CurrentPageIndex;
            if (gvChargePayment.DisplayLayout.SelectedRows[0] != null)
            {
                if (Int32.TryParse(gvChargePayment.DisplayLayout.SelectedRows[0].Cells[6].Value.ToString(), out TransctionType))
                {

                    if (Int32.TryParse(gvChargePayment.DisplayLayout.SelectedRows[0].Cells[7].Value.ToString(), out iHistoryId))
                        this.popChargeHistory.iHistoryId = iHistoryId;
                    if (Boolean.TryParse(gvChargePayment.DisplayLayout.SelectedRows[0].Cells[8].Value.ToString(), out bIsOneTime))
                        this.popChargeHistory.IsOneTime = bIsOneTime;
                    if (TransctionType == 0)
                    {
                        this.popChargeHistory.IsEditable = false;
                        this.popChargeHistory.IsCharge = true;
                        this.popChargeHistory.WinState = true;
                        this.popChargeHistory.IsReversePayment = false;

                    }
                    else if (TransctionType == 1)
                    {
                        this.popChargeHistory.IsCharge = false;
                        this.popChargeHistory.IsPayment = true;
                        this.popChargeHistory.IsEditable = false;
                        this.popChargeHistory.IsCheck = false;
                        this.popChargeHistory.WinState = true;
                        this.popChargeHistory.IsReversePayment = false;
                    }
                    else if (TransctionType == 2)
                    {
                        this.popChargeHistory.IsCharge = false;
                        this.popChargeHistory.IsCredit = true;
                        this.popChargeHistory.IsPayment = false;
                        this.popChargeHistory.IsCheck = false;
                        this.popChargeHistory.IsEditable = false;
                        this.popChargeHistory.IsReversePayment = false;
                        this.popChargeHistory.WinState = true;
                    }
                    else if (TransctionType == 3)
                    {
                        this.popChargeHistory.IsCharge = false;
                        this.popChargeHistory.IsCredit = false;
                        this.popChargeHistory.IsPayment = false;
                        this.popChargeHistory.IsEditable = false;
                        this.popChargeHistory.IsReversePayment = false;
                        this.popChargeHistory.IsCheck = true;
                        this.popChargeHistory.WinState = true;
                    }
                    else if (TransctionType == 4)
                    {
                        this.popChargeHistory.IsCharge = false;
                        this.popChargeHistory.IsCredit = false;
                        this.popChargeHistory.IsPayment = false;
                        this.popChargeHistory.IsEditable = false;
                        this.popChargeHistory.IsCheck = false;
                        this.popChargeHistory.IsReversePayment = true;
                        this.popChargeHistory.WinState = true;
                    }
                }
            }
            else
            {
                Master.AllowMessagePopupPostback = false;
                Master.Message = Constant.SELECT_RECORD;
                Master.MessageTitle = Constant.INFO;
            }
        }

        protected void lnkAddRecord_Click(object sender, EventArgs e)
        {
            this.popChargeHistory.IsCharge = true;
            this.popChargeHistory.IsEditable = true;
            this.popChargeHistory.WinState = true;
            this.popChargeHistory.dBalance = dBalance;
        }

        protected void lnkUpdateFinancial_Click(object sender, EventArgs e)
        {
            try
            {
                MembersBL thisMemberCheck = MembersBL.getByUserId(securityCheck.iMemberId);
                AppLogger.logInfo("Start: MeridianEHealth.Personal.lnkUpdateFinancial_Click()");

                if (!trIndividual.Visible && validateHeader())
                {
                    MembersBL thisMember = getMember();
                    thisMember.Save();
                    Master.MessageTitle = Constant.SUCCESS;
                    Master.Message = Constant.SAVE_SUCCESS;
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateFinancial_Click()");
                    return;
                }
                if (thisMemberCheck.iMemberType != 7)
                {
                    if (!validateFinanceInfo())
                    {
                        AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateFinancial_Click()");
                        return;
                    }
                    if (!Utils.Validate(ddlCreditCardName.Text, txtNumber.Text))
                    {
                        Master.MessageTitle = Constant.ERROR;
                        Master.Message = Constant.WRONG_FORMAT;
                        return;
                    }
                    MemberFinanceBL objMember = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
                    if (objMember != null && objMember.bPayByCheque)
                    {
                        activateAccount();
                    }
                    if (saveFinancialInfo())
                    {
                        Master.AllowMessagePopupPostback = true;
                        Master.MessageTitle = Constant.SUCCESS;
                        Master.Message = Constant.SAVE_SUCCESS + strCreditError;
                        AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateFinancial_Click()");
                        return;
                    }
                }
                return;
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.btnAdd_Click()");

                ListItemCollection items = new ListItemCollection();

                AppLogger.logInfo("End: MeridianEHealth.Personal.btnAdd_Click()");
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.btnRemove_Click()");

                ListItemCollection items = new ListItemCollection();

                AppLogger.logInfo("End: MeridianEHealth.Personal.btnRemove_Click()");
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void lnkChangeSponsor_click(object sender, EventArgs e)
        {
            if (CheckReadOnly())
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.READ_ONLY;
            }
            else
            {
                Response.Redirect("~/AddinalUserSignUp.aspx");
            }

        }

        protected void lnkUpdateCondition_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.lnkUpdateCondition_Click()");

                if (!validateConditionInfo())
                {
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateCondition_Click()");
                    return;
                }
                if (saveConditionInfo())
                {
                    Master.MessageTitle = Constant.SUCCESS;
                    Master.Message = Constant.SAVE_SUCCESS;
                    AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateCondition_Click()");
                    return;
                }

                AppLogger.logInfo("End: MeridianEHealth.Personal.lnkUpdateCondition_Click()");
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void lnkUpdateSecurity_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo(AppLoggerConstant.start_Personal_lnkUpdateSecurity_Click);
                //PHN_PERFORMANCE //MembersBL thisMembersBL = MembersBL.getByUserId(securityCheck.iUserId);
                MembersBL thisMembersBL = securityCheck.objUser;
                if (thisMembersBL != null)
                {
                    if (!String.IsNullOrEmpty(txtOldPassword.Text) || !String.IsNullOrEmpty(ucNewPassword.ucPassword_Text) || !String.IsNullOrEmpty(txtReTypeNewPassword.Text) || (chkSecurityQst.Checked == true) || (thisMembersBL.iMemberType.HasValue ? thisMembersBL.iMemberType.Value : 0) == Convert.ToInt32(MemberType.MS_Caregiver))
                    {
                        if (!validateSecurityInfo())
                        {
                            ClearSecurityDate();
                            AppLogger.logInfo(AppLoggerConstant.end_Personal_lnkUpdateSecurity_Click);
                            return;
                        }
                        if (saveSecurityInfo())
                        {
                            Master.AllowMessagePopupPostback = false;
                            Master.MessageTitle = Constant.SUCCESS;
                            Master.Message = Constant.SAVE_SUCCESS;
                            ClearSecurityDate();
                            AppLogger.logInfo(AppLoggerConstant.end_Personal_lnkUpdateSecurity_Click);
                            return;
                        }
                        else
                        {
                            Master.AllowMessagePopupPostback = false;
                            Master.MessageTitle = Constant.INFO;
                            Master.Message = Constant.PASSWORD_CHANGE_FAIL;
                            ClearSecurityDate();
                            AppLogger.logInfo(AppLoggerConstant.end_Personal_lnkUpdateSecurity_Click);
                            return;
                        }
                    }
                    else
                    {
                        Master.AllowMessagePopupPostback = false;
                        Master.MessageTitle = Constant.INFO;
                        Master.Message = Constant.FILL_SECURITY_INFO;
                        AppLogger.logInfo(AppLoggerConstant.end_Personal_lnkUpdateSecurity_Click);
                        return;
                    }
                }
            }
            catch (System.Data.ReadOnlyException exp)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = exp.Message;
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void rdolstCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                loadCondition();
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void lnkbtnConditionSearch_Click(object sender, EventArgs e)
        {
            try
            {
                loadCondition();
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.SYTEM_ERROR;
            }
        }

        protected void rblChangePayee_SelectedIndexChanged(object sender, EventArgs e)
        {
            Int32 iAge;
            if (rblChangePayee.SelectedValue != null && Convert.ToInt32(rblChangePayee.SelectedValue) == Convert.ToInt32(Payee.Own))
            {
                trIndividual.Visible = true;

                MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);

                MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
                if (thisMember.dtDateOfBirth.HasValue)
                    iAge = Age.Year(thisMember.dtDateOfBirth.Value);
                else
                    iAge = 0;
                if (iAge >= 18)
                {
                    ddlPlans.Visible = true;
                    lblPlanSelected.Visible = false;
                    Creditcardvisibility(true);
                    ClearCreditCard();
                }
            }
            else if (rblChangePayee.SelectedValue != null && Convert.ToInt32(rblChangePayee.SelectedValue) == Convert.ToInt32(Payee.Parent))
            {
                List<AdditionalMembersBL> lstAdditionalMembers = AdditionalMembersBL.getDataByMemberId(securityCheck.iMemberId);
                if (lstAdditionalMembers != null && lstAdditionalMembers.Count == 1)
                {
                    foreach (AdditionalMembersBL thisAdditionalMembers in lstAdditionalMembers)
                    {
                        lblPlanSelected.Visible = true;
                        ddlPlans.Visible = false;
                        MemberFinanceBL thisNewMemberFinance = MemberFinanceBL.getByMemberId(thisAdditionalMembers.iMemberID);
                        if (thisNewMemberFinance != null)
                            fillCreditCardInfo(thisNewMemberFinance);
                        Creditcardvisibility(false);
                    }
                }
            }
        }

        protected void lnkChangeMemberships_Click(object sender, EventArgs e)
        {
            if (CheckReadOnly())
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = Constant.READ_ONLY;
            }
            else
            {
                SessionInfo.ReferralPath.Obj = "~/Personal.aspx";
                Response.Redirect("~/ChangeMembership.aspx");
            }
        }

        protected void lnkCloseAccount_Click(object sender, EventArgs e)
        {
            MembersBL thisMembersBL = MembersBL.getByUserId(securityCheck.iMemberId);
            if (thisMembersBL != null)
            {
                Int32 iClinicianid = securityCheck.iUserId;
                securityCheck.iUserId = securityCheck.iMemberId;
                thisMembersBL.iActive = Convert.ToInt32(MemberStatus.InActive);
                thisMembersBL.Save();

                if (SessionInfo.BackPage.SetInSession)
                {
                    SessionInfo.isDeactivated.Obj = true;
                    securityCheck.iMemberId = iClinicianid;
                    securityCheck.iUserId = iClinicianid;
                    String backpage = SessionInfo.BackPage.str;
                    Response.Redirect(backpage, true);
                }
            }
        }

        protected void lnkWhatArethis_Click(object sender, EventArgs e)
        {

        }

        protected void BindProduct()
        {
            MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
            if (thisMemberFinance != null)
            {
                List<MemberProductBL> _listProduct = MemberProductBL.getDataByMemberFinanceId(thisMemberFinance.iId);
                if (_listProduct != null)
                {
                    gvFinancialInfo.DataSource = _listProduct;
                    gvFinancialInfo.DataBind();
                }
            }
        }

        protected void BindMyNumbers()
        {
            List<MemberNumbersBL> lstMemberNumbersBL = MemberNumbersBL.GetListByMemberId(securityCheck.iMemberId);
            if (lstMemberNumbersBL != null)
            {
                gvMyNumbers.DataSource = lstMemberNumbersBL;
                gvMyNumbers.DataBind();
            }
        }

        protected void gvMyNumbers_InitializeRow(object sender, RowEventArgs e)
        {
            //UltraGridCell _iNumberType = e.Row.Cells.FromKey("iNumberTypeId");
            //if (_iNumberType != null)
            //{
            //    Int32 iNumberType = ((Meridian.BL.MemberNumbersBL)(e.Data)).iNumberTypeId.Value;
            //    if (iNumberType == Convert.ToInt32(NumberType.InboundPersonal))
            //    {
            //        _iNumberType.Text = NumberType.InboundPersonal.ToString();
            //    }
            //    else if(iNumberType == Convert.ToInt32(NumberType.InboundGroup))
            //    {
            //        _iNumberType.Text = NumberType.InboundGroup.ToString();
            //    }
            //    else if(iNumberType == Convert.ToInt32(NumberType.Outbound))
            //    {
            //        _iNumberType.Text = NumberType.Outbound.ToString();
            //    }
            //}

            if (e.Data as MemberNumbersBL != null)
            {
                String strNumber = (e.Data as MemberNumbersBL).strNumber;

                UltraGridCell Description = e.Row.Cells.FromKey("Description");

                if (Description != null && !String.IsNullOrEmpty(strNumber))
                {
                    if (strNumber.Contains("TollFree") || strNumber.Substring(1, 1) == "8" && (strNumber.Substring(2, 1) == strNumber.Substring(3, 1)))
                    {
                        Description.Text = "Toll Free";
                    }
                    else
                    {
                        Description.Text = "Local";
                    }
                }

                UltraGridCell TextEnabled = e.Row.Cells.FromKey("bText_Enabled");
                if (TextEnabled != null)
                {
                    if ((e.Data as MemberNumbersBL).bText_Enabled.HasValue && (e.Data as MemberNumbersBL).bText_Enabled.Value)
                    {
                        TextEnabled.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Constant.YES.ToLower());
                    }
                    else
                    {
                        TextEnabled.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Constant.NO.ToLower());
                    }
                }

                UltraGridCell E911Enabled = e.Row.Cells.FromKey("bE911_Enabled");
                if (E911Enabled != null)
                {
                    if ((e.Data as MemberNumbersBL).bE911_Enabled.HasValue && (e.Data as MemberNumbersBL).bE911_Enabled.Value)
                    {
                        E911Enabled.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Constant.YES.ToLower());
                    }
                    else
                    {
                        E911Enabled.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Constant.NO.ToLower());
                    }
                }

                UltraGridCell Number = e.Row.Cells.FromKey("strNumber");
                if (Number != null)
                {
                    try
                    {
                        if (strNumber.Trim().Contains(Constant.ON_ORDER))
                        {
                            Number.Text = Constant.ON_ORDER;
                        }
                        else
                        {
                            E164 objPhoneNumber = E164.validate(strNumber.Trim());
                            String E164PhoneNumber = objPhoneNumber.formatNANPTn(E164.PATTERN_PARENS, false, false);
                            Number.Text = E164PhoneNumber;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        protected void gvChargePayment_OnPageIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.gvChargePayment_OnPageIndexChanged()");
                LoadPaymentAndChargeData();
                AppLogger.logInfo("End: MeridianEHealth.Personal.gvChargePayment_OnPageIndexChanged()");
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                AppLogger.logInfo("End: MeridianEHealth.Personal.gvChargePayment_OnPageIndexChanged()");
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void gvUsageHistory_OnPageIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Personal.gvUsageHistory_OnPageIndexChanged()");
                BindUsageHistory();
                AppLogger.logInfo("End: MeridianEHealth.Personal.gvUsageHistory_OnPageIndexChanged()");
            }
            catch (Exception exp)
            {
                Utils.sendExceptionMail(exp);
                AppLogger.logInfo("End: MeridianEHealth.Personal.gvChargePayment_OnPageIndexChanged()");
                ExceptionPolicy.HandleException(exp, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void gvUsageHistory_InitializeRow(object sender, RowEventArgs e)
        {
            try
            {
                System.Web.UI.WebControls.Label lblCharge = null;
                UltraGridCell _cellMembername = e.Row.Cells.FromKey("dCharge");
                TemplatedColumn _objTemplatedColumnsMembername = (TemplatedColumn)_cellMembername.Column;
                CellItem _objCellItemMembername = (CellItem)_objTemplatedColumnsMembername.CellItems[e.Row.Index];
                if (_objCellItemMembername != null)
                {
                    if (((Meridian.BL.ssPhnComfoneCdrsBL)(e.Data)).dCharge.HasValue)
                    {
                        String dCharge = ((Meridian.BL.ssPhnComfoneCdrsBL)(e.Data)).dCharge.ToString();
                    }
                    lblCharge = _objCellItemMembername.FindControl("lblCharge") as System.Web.UI.WebControls.Label;
                    if (lblCharge != null && _cellMembername.Value != null)
                        lblCharge.Text = '$' + _cellMembername.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        protected void lnkUpdate_Click(object sender, EventArgs e)
        {

            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Sponsor.Applications.lnkUpdate_Click()");
                //MembersBL thisMembersBL = MembersBL.getByUserId(securityCheck.iMemberId);
                MembersBL thisMembersBL = securityCheck.objMember;
                if (thisMembersBL != null)
                {
                    if (ddlUpdateStatus1.Visible)
                    {
                        if (thisMembersBL != null && thisMembersBL.iActive == Convert.ToInt32(MemberStatus.OnHold) && ddlUpdateStatus1.SelectedValue == Convert.ToInt32(MemberStatus.Active).ToString())
                        {
                            SendMail(MailType.Approve);
                        }
                        else
                        {
                            thisMembersBL.iActive = Convert.ToInt32(ddlUpdateStatus1.SelectedValue);
                        }
                    }
                    else if (ddlUpdateStatus2.Visible)
                    {
                        if (thisMembersBL != null && thisMembersBL.iActive == Convert.ToInt32(MemberStatus.OnHold) && ddlUpdateStatus2.SelectedValue == Convert.ToInt32(MemberStatus.Active).ToString())
                        {
                            SendMail(MailType.Approve);
                        }
                        else
                        {
                            thisMembersBL.iActive = Convert.ToInt32(ddlUpdateStatus2.SelectedValue);
                        }
                    }
                    if (thisMembersBL.Save())
                    {
                        Master.MessageTitle = Constant.SUCCESS;
                        Master.Message = Constant.SAVE_SUCCESS;
                        Master.AllowMessagePopupPostback = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }


        }

        #endregion

        #region "Private"

        private void LoadLanguagesDropdownList()
        {
            try
            {
                List<LanguageBL> lstLanguageBL = LanguageBL.getData();
                if (lstLanguageBL != null && lstLanguageBL.Count > 0)
                {
                    ddlLanguage.DataSource = lstLanguageBL;
                    ddlLanguage.DataTextField = Constant.STR_LANGUAGE_NAME;
                    ddlLanguage.DataValueField = Constant.IID;
                    ddlLanguage.DataBind();
                    ListItem removeLanguageItem = ddlLanguage.Items.FindByText(Constant.ALL);
                    ddlLanguage.Items.Remove(removeLanguageItem);
                }
                ListItem liItem = new ListItem(String.Empty, Constant.ZERO);
                ddlLanguage.Items.Insert(0, liItem);

            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }
        }

        private void checkScope(Boolean bFlag)
        {
            try
            {
                if (!bFlag)
                    Response.Redirect("~/NoRights.aspx", true);
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
        }

        private void ShowHidePlanType()
        {
            MemberFinanceBL thisFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
            if (thisFinance != null)
            {
                List<MemberProductBL> lstProducts = MemberProductBL.getDataByMemberFinanceId(thisFinance.iId);
                {
                    if (lstProducts != null && lstProducts.Count > 0)
                    {
                        foreach (MemberProductBL _objProduct in lstProducts)
                        {
                            ProductPricingBL thisProductPrice = ProductPricingBL.GetPriceForNumberofMembers(_objProduct.iBrandProductId, 1);
                            if (thisProductPrice != null && (!thisProductPrice.dContract.HasValue ||
                                (thisProductPrice.dContract.HasValue && thisProductPrice.dContract.Value == 0)))
                            {
                                if (thisProductPrice.dMonthly.HasValue && !ddlPlans.Items.Contains(new ListItem("1 Month", "1")))
                                    ddlPlans.Items.Add(new ListItem("1 Month", "1"));

                                if (thisProductPrice.dQuarterly.HasValue && !ddlPlans.Items.Contains(new ListItem("3 Months", "2")))
                                    ddlPlans.Items.Add(new ListItem("3 Months", "2"));

                                if (thisProductPrice.dSemiAnnual.HasValue && !ddlPlans.Items.Contains(new ListItem("6 Month", "3")))
                                    ddlPlans.Items.Add(new ListItem("6 Month", "3"));

                                if (thisProductPrice.dAnnual.HasValue && !ddlPlans.Items.Contains(new ListItem("1 Year", "4")))
                                    ddlPlans.Items.Add(new ListItem("1 Year", "4"));
                            }
                        }
                        if (ddlPlans.Items.Count == 0)
                        {
                            trPlanType.Visible = false;
                        }
                    }
                }
            }
        }

        private void setColor()
        {
            BrandBL objBrand = BrandBL.getBrand();
            //for financial tab
            gvFinancialInfo.DisplayLayout.HeaderStyleDefault.BackColor = Color.FromName(objBrand.strButton);
            gvFinancialInfo.DisplayLayout.Pager.PagerStyle.BackColor = Color.FromName(objBrand.strButton);
            //for charge/payment tab
            gvChargePayment.DisplayLayout.HeaderStyleDefault.BackColor = Color.FromName(objBrand.strButton);
            gvChargePayment.DisplayLayout.Pager.PagerStyle.BackColor = Color.FromName(objBrand.strButton);
            gvChargePayment.DisplayLayout.SelectTypeCellDefault = Infragistics.WebUI.UltraWebGrid.SelectType.None;
            gvChargePayment.DisplayLayout.SelectTypeRowDefault = Infragistics.WebUI.UltraWebGrid.SelectType.Single;
            gvChargePayment.DisplayLayout.SelectTypeColDefault = Infragistics.WebUI.UltraWebGrid.SelectType.None;
            gvChargePayment.DisplayLayout.CellClickActionDefault = Infragistics.WebUI.UltraWebGrid.CellClickAction.RowSelect;
            //For Usage History tab
            gvUsageHistory.DisplayLayout.HeaderStyleDefault.BackColor = Color.FromName(objBrand.strButton);
            gvUsageHistory.DisplayLayout.Pager.PagerStyle.BackColor = Color.FromName(objBrand.strButton);

            //For My Numbers
            gvMyNumbers.DisplayLayout.HeaderStyleDefault.BackColor = Color.FromName(objBrand.strButton);
            gvMyNumbers.DisplayLayout.Pager.PagerStyle.BackColor = Color.FromName(objBrand.strButton);

        }

        private void checkpreviousPage()
        {
            if (SessionInfo.PreviousPage.id.HasValue && SessionInfo.PreviousPage.id == Convert.ToInt32(ApplicationRedirect.ExpressSetup))
            {
                UltraWebTab1.SelectedTab = 1;
                SessionInfo.PreviousPage.Unset();
            }
        }

        private void configureTab()
        {
            if (!securityCheck.checkScope(Constant.SCOPE_CONTACT) &&
               !securityCheck.checkScope(Constant.SCOPE_CONDITIONS) &&
               !securityCheck.checkScope(Constant.SCOPE_SECURITY) &&
               !securityCheck.checkScope(Constant.SCOPE_FINANCIAL) &&
                !securityCheck.checkScope(Constant.SCOPE_CHARGE_PAYMENT) &&
                !securityCheck.checkScope(Constant.SCOPE_USAGE_HISTORY))
            {
                UltraWebTab1.Visible = false;
                lnkUpdateContact.Visible = false;
            }
            else
            {
                UltraWebTab1.Visible = true;
                lnkUpdateContact.Visible = true;
            }
            //hide update member button
            lnkUpdateMember.Visible = !UltraWebTab1.Visible;
        }

        private void configureContact(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_CONTACT).Visible = bFlag;
        }

        private void configureCondition(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_CONDITIONS).Visible = bFlag;
        }

        private void configureSecurtiy(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_SECURITY).Visible = bFlag;
        }

        private void configurePayment(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_CHARGE_PAYMENT).Visible = bFlag;
        }

        private void configureUsage(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_USAGE_HISTORY).Visible = bFlag;
        }

        private void confirureMyNumbers(Boolean bFlag)
        {
            UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_MY_NUMBERS).Visible = bFlag;
        }

        private void configureFinance(Boolean bFlag)
        {
            if (bFlag)
            {
                UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_FINANCIAL).Visible = bFlag;
            }
            else
            {
                MembersBL thisMember = MembersBL.getByUserId(SessionInfo.Member.id.Value);
                MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(SessionInfo.Member.id.Value);
                if (thisMember != null && thisMemberFinance != null)
                {
                    if (thisMember.iActive == Convert.ToInt32(MemberStatus.InActive) &&
                        thisMember.iSponserId == BrandBL.objBrand.iPrimarySponsor &&
                        !thisMemberFinance.iBrandProductId.HasValue)
                    {
                        UltraWebTab1.Visible = true;
                        UltraWebTab1.Tabs.FromKeyTab(Constant.SCOPE_FINANCIAL).Visible = true;

                        if (trMembershipsDesired != null)
                            trMembershipsDesired.Visible = false;
                    }
                }
            }
        }

        private void configureHistory()
        {
            if (securityCheck.iUserId != securityCheck.iMemberId)
            {
                //MembersBL thisMember = MembersBL.getByUserId(securityCheck.iUserId);
            //    if (securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Administrator_Caregiver) || securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Caregiver))
            //        //lnkAddRecord.Visible = true;
            //    else
            //        //lnkAddRecord.Visible = false;
            //}
            //else
            //    //lnkAddRecord.Visible = false;
            }
        }

        private void loadCondition()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.loadCondition()");
            Int32 iProductId = 0;
            MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
            if (thisMemberFinance != null && thisMemberFinance.iBrandProductId.HasValue)
                iProductId = thisMemberFinance.iBrandProductId.Value;

            AppLogger.logInfo("End: MeridianEHealth.Personal.loadCondition()");
        }

        private void loadStates()
        {

            AppLogger.logInfo("Start: MeridianEHealth.Personal.loadStates()");
            List<StateBL> _lstStates = StateBL.getData();
            if (_lstStates != null)
            {
                ddlFinState.DataSource = _lstStates;
                ddlFinState.DataTextField = "strStateCode";
                ddlFinState.DataValueField = "strStateCode";
                ddlFinState.DataBind();
                ddlFinState.Items.Insert(0, "");
            }

            AppLogger.logInfo("End: MeridianEHealth.Personal.loadStates()");
        }

        private void loadStatus()
        {
            AppLogger.logInfo("Start: MeridianEHealth.MyMember.loadStatus()");
            Dictionary<int, string> pairs = new Dictionary<int, string>();
            foreach (int i in Enum.GetValues(typeof(MemberStatus)))
            {
                if (i == Convert.ToInt32(Meridian.BL.Status.Active) || i == Convert.ToInt32(MemberStatus.InActive) || i == Convert.ToInt32(MemberStatus.OnHold))
                    pairs.Add(i, Enum.GetName(typeof(MemberStatus), i));
            }

            ddlUpdateStatus1.DataSource = pairs;
            ddlUpdateStatus2.DataSource = pairs;
            ddlUpdateStatus1.DataTextField = "Value";
            ddlUpdateStatus2.DataTextField = "Value";
            ddlUpdateStatus1.DataValueField = "Key";
            ddlUpdateStatus2.DataValueField = "Key";
            ddlUpdateStatus1.DataBind();
            ddlUpdateStatus2.DataBind();
            Utils.sortDDL(ref this.ddlUpdateStatus1);
            Utils.sortDDL(ref this.ddlUpdateStatus2);
            AppLogger.logInfo("End: MeridianEHealth.MyMember.loadStatus()");

        }

        private void loadCreditCard()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.loadCreditCard()");

            List<CreditCardBL> _lstCreditCard = CreditCardBL.getData();
            if (_lstCreditCard != null)
            {
                ddlCreditCardName.DataSource = _lstCreditCard;
                ddlCreditCardName.DataTextField = "strCreditCard";
                ddlCreditCardName.DataValueField = "iId";
                ddlCreditCardName.DataBind();
            }

            AppLogger.logInfo("End: MeridianEHealth.Personal.loadCreditCard()");
        }

        private void assignValidationGroup(string strValidationGroup)
        {
            RangeValidatorYear.ValidationGroup = strValidationGroup;
            RangeValidatorDate.ValidationGroup = strValidationGroup;
            RangeValidatorMonth.ValidationGroup = strValidationGroup;
            RequiredFieldFirstName.ValidationGroup = strValidationGroup;
        }

        private void fillHeaderInfo(MembersBL thisMember)
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.fillHeaderInfo()");
            DateTime dtBirthDate = new DateTime();

            ddlUpdateStatus1.SelectedValue = thisMember.iActive.Value.ToString();
            ddlUpdateStatus2.SelectedValue = thisMember.iActive.Value.ToString();
            txtMiddle.Text = thisMember.strInitial;
            txtFirstName.Text = thisMember.strFirstName;
            lblUser_id.Text = thisMember.strUserId;
            txtPhoneticFirstName.Text = thisMember.strPhoneticFirstName;
            txtPhoneticLastName.Text = thisMember.strPhoneticLastName;

            if (thisMember.iLanguageid.HasValue && thisMember.iLanguageid.Value != 0)
            {
                ddlLanguage.SelectedValue = thisMember.iLanguageid.Value.ToString();
            }
            else
            {
                ddlLanguage.SelectedValue = ddlLanguage.Items.FindByText(Constant.ENGLISH).Value.ToString();
            }
            if (thisMember.iActive == 1)
            {
                lblStatusValue.Text = "Active";
            }
            else if (thisMember.iActive == 0)
            {
                lblStatusValue.Text = "InActive";
            }
            else if (thisMember.iActive == 4)
            {
                lblStatusValue.Text = "On Hold";
            }
            txtLastName.Text = thisMember.strLastName;
            if (thisMember.dtDateOfBirth != null)
            {
                dtBirthDate = Convert.ToDateTime(thisMember.dtDateOfBirth);
                txtMonth.Text = dtBirthDate.Month.ToString();
                txtDate.Text = dtBirthDate.Day.ToString();
                txtYear.Text = dtBirthDate.Year.ToString();
            }
            if (thisMember.bGender.HasValue)
            {
                ddlGender.SelectedValue = thisMember.bGender.Value ? "true" : "false";
            }
            else
            {
                ddlGender.SelectedValue = "0";
            }

            if (!String.IsNullOrEmpty(thisMember.strHomePhone))
            {
                //lblHomePhoneNo.Text = thisMember.strHomePhone;

                objHomePhone = E164.validate(thisMember.strHomePhone);
                objHomePhone.formatNANPTn(E164.PATTERN_PARENS, false, false);

                AssignValues(2, objHomePhone);
                thisMember.strHomePhone = objHomePhone == null ? null : objHomePhone.getDigits();
                lblHomePhoneNo.Text = objHomePhone == null ? null : objHomePhone.formatNANPTn(E164.PATTERN_PARENS, false, false);

            }
            if (!String.IsNullOrEmpty(thisMember.strWorkPhone))
            {
                //lblWorkPhoneNo.Text = thisMember.strWorkPhone;

                objWorkPhone = E164.validate(thisMember.strWorkPhone);
                objWorkPhone.formatNANPTn(E164.PATTERN_PARENS, false, false);

                AssignValues(3, objWorkPhone);
                thisMember.strWorkPhone = objWorkPhone == null ? null : objWorkPhone.getDigits();
                lblWorkPhoneNo.Text = objWorkPhone == null ? null : objWorkPhone.formatNANPTn(E164.PATTERN_PARENS, false, false);

            }

            txteMailAlias.Text = thisMember.strEmail;

            if (!String.IsNullOrEmpty(thisMember.strQuestion))
            {
                ddlQuestion.SelectedValue = thisMember.strQuestion.ToString();
            }
            else
            {
                ddlQuestion.SelectedValue = String.Empty;
            }

            if (securityCheck.iMemberId != securityCheck.iUserId)
            {
                //PHN_PERFORMANCE
                //MembersBL objMember = MembersBL.getByUserId(securityCheck.iUserId);
                if (securityCheck.objUser != null && (securityCheck.objUser.iMemberType == 2 || securityCheck.objUser.iMemberType == 5))
                {
                    trOldPassword.Visible = false;
                }
                else
                {
                    trOldPassword.Visible = true;
                }
            }
            else
            {
                trOldPassword.Visible = true;
            }
            AppLogger.logInfo("End: MeridianEHealth.Personal.fillHeaderInfo()");
        }

        private void fillContactInformationInfo(MembersBL thisMember)
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.fillContactInformationInfo()");


            E164 objNumber = formatNumber(thisMember.strHomePhone);
            if (objNumber != null)

                objNumber = formatNumber(thisMember.strWorkPhone);
            objNumber = formatNumber(thisMember.strCellPhone);
            objNumber = formatNumber(thisMember.strEmergencyPhone);

            if (thisMember.bMemberList.HasValue)
            {
                chkIncludeName.Checked = thisMember.bMemberList.HasValue ? thisMember.bMemberList.Value : false;
            }

            AppLogger.logInfo("End: MeridianEHealth.Personal.fillContactInformationInfo()");
        }

        private void fillConditionInfo(MembersBL thisMember)
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.fillConditionInfo()");

            AppLogger.logInfo("End: MeridianEHealth.Personal.fillConditionInfo()");
        }

        private void setMembershipTypeForCoach()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.setMembershipTypeForCoach()");

            trMemberInfo.Visible = false;
            lblMembersshipType.Text = "Coach Only";

            AppLogger.logInfo("Start: MeridianEHealth.Personal.setMembershipTypeForCoach()");
        }

        private void fillfinanceInfo(MembersBL thisMember)
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.fillfinanceInfo()");

            if (securityCheck.iMemberType == Convert.ToInt32(MemberType.Coach))
            {
                setMembershipTypeForCoach();
                lnkChangeToMember.Visible = true;
                return;
            }
            Int32 iAge;
            lblMembersshipType.Text = "Full Member";
            MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);

            if (thisMemberFinance == null)// !thisMemberFinance.iBrandProductId.HasValue
            {
                trChangePayee.Visible = false;
                // if (!thisMemberFinance.iBrandProductId.HasValue)
                lblMembersshipType.Text = "";
                return;
            }
            if (thisMemberFinance != null)
            {
                if (thisMember.iPayeeID.HasValue)
                    rblChangePayee.SelectedIndex = Convert.ToInt32(Payee.Own);
                else
                    rblChangePayee.SelectedIndex = Convert.ToInt32(Payee.Parent);


                if (thisMember.iMemberType != Convert.ToInt32(MemberType.UserOnly))
                    lnkChangeSponsor.Visible = false;

                if (thisMember.iMemberType != Convert.ToInt32(MemberType.Coach))
                    lnkChangeSponsor.Visible = true;


                if (thisMember.dtDateOfBirth.HasValue)
                    iAge = Age.Year(thisMember.dtDateOfBirth.Value);
                else
                    iAge = 0;

                if (iAge > 18)
                {
                    lnkChangeSponsor.Visible = true;
                    lnkChangeMemberships.Visible = true;

                    if (thisMember.iSponsorProgramId.HasValue)
                    {
                        SponsorProgramBL thisProgram = SponsorProgramBL.getDataById(thisMember.iSponsorProgramId.Value);

                        if (thisProgram != null)
                        {
                            if (thisMember.bIsDependent && thisProgram.iPayBy.Value != Convert.ToInt32(PaymentApproach.SponsorAlways) && thisMember.iSponserId != 1)
                                trChangePayee.Visible = true;
                            else
                                trChangePayee.Visible = false;
                        }
                        else
                            trChangePayee.Visible = false;
                    }
                    else
                    {
                        if (!thisMember.bIsDependent)
                            trChangePayee.Visible = false;
                    }
                }
                else
                {
                    trChangePayee.Visible = false;
                    lnkChangeSponsor.Visible = false;
                    if (!thisMember.bIsDependent)
                    {
                        lnkChangeMemberships.Visible = false;
                    }
                }

                if (thisMember.bIsDependent)
                {
                    AdditionalMembersBL _AdditionalMember = AdditionalMembersBL.getRelatedMemberId(securityCheck.iMemberId);
                    if (_AdditionalMember != null)
                    {
                        Int32 _iPrimaryMemberID = _AdditionalMember.iPrimaryMemberID;
                        MembersBL _ParentMember = MembersBL.getByUserId(_iPrimaryMemberID);
                    }

                    List<InvitationBL> lstInvitations = InvitationBL.getMyRequests(thisMember.iId);

                    if (lstInvitations != null)
                    {
                        if (lstInvitations.Count > 0)
                        {
                            lblMembershipDesired.Text = "Your Offers:";
                            txtMemberShipDesired.Text = lstInvitations.Count.ToString();
                            lnkChangeMemberships.Text = "Offers To Pay";
                        }
                    }
                    else
                    {
                        tdlblMembershipDesired.Style.Add("display", "none");
                        tdlnkchangememberships.Style.Add("display", "none");
                        tdtxtmembershipdesired.Style.Add("display", "none");
                    }

                    // don't show the grid to additional members
                    trChangePayee.Visible = false;

                }

                Nullable<Int32> cntCoaches = MembersBL.getAvailableCoachCount(thisMember.iId, Convert.ToInt32(MemberType.Coach));



                if (thisMemberFinance.dtFreePeriodEndOn.HasValue)
                {
                    DateTime _dtFreePeriod = thisMemberFinance.dtFreePeriodEndOn.Value;
                }
                Int32 _iAdditionalMemCount = MembersBL.GetAdditionalMemberCount(securityCheck.iMemberId);
                txtMemberShipDesired.Text = (_iAdditionalMemCount + 1).ToString();
            }
            else
            {
                txtMemberShipDesired.Text = "1";
            }
            if (ShowCreditCardInfo(thisMember))
            {
                lblPlanSelected.Visible = false;
                ddlPlans.Visible = true;
                trIndividual.Visible = true;

                fillCreditCardInfo(thisMemberFinance);
            }
            else
            {
                if (securityCheck.iUserId != securityCheck.iMemberId)
                {
                    //PHN_PERFORMACNE
                    //MembersBL thisMembersBL = MembersBL.getByUserId(securityCheck.iUserId);
                    if (securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Administrator_Caregiver) || securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Caregiver))
                    {
                        //lnkCloseAccount1.Visible = true;
                        ddlUpdateStatus1.Visible = true;
                        lnkUpdateStatus1.Visible = true;
                    }
                }
            }

            if (thisMember.iMemberType == (Int32)MemberType.MS_Administrator || thisMember.iMemberType == (Int32)MemberType.MS_Administrator_Caregiver || thisMember.iMemberType == (Int32)MemberType.MS_Caregiver || thisMember.iMemberType == (Int32)MemberType.SP_Admistrator_User)
            {
                lnkChangeSponsor.Visible = false;
            }

            AppLogger.logInfo("End: MeridianEHealth.Personal.fillfinanceInfo()");
        }

        private void BillingControlShow()
        {
            //PHN_PERFORMANCE
            //MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
            if (securityCheck.objMember != null && securityCheck.objMember.bIsDependent == true)
            {
                lblNextBillingDate.Visible = false;

            }
        }

        private void fillCreditCardInfo(MemberFinanceBL thisMemberFinance)
        {

            trIndividual.Visible = true;
            if (securityCheck.iUserId != securityCheck.iMemberId)
            {
                //PHN_PERFORMANCE
                //MembersBL thisMembersBL = MembersBL.getByUserId(securityCheck.iUserId);
                if (securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Administrator_Caregiver) || securityCheck.objUser.iMemberType == Convert.ToInt32(MemberType.MS_Caregiver))
                {
                    //lnkCloseAccount2.Visible = true;
                    ddlUpdateStatus2.Visible = true;
                    lnkUpdateStatus2.Visible = true;
                }
            }
            if (thisMemberFinance.iCreditCardId.HasValue)
            {
                ddlCreditCardName.SelectedValue = thisMemberFinance.iCreditCardId.HasValue ? thisMemberFinance.iCreditCardId.Value.ToString() : "1";
                if (!String.IsNullOrEmpty(thisMemberFinance.strState))
                    ddlFinState.SelectedValue = Security.decode(thisMemberFinance.strState);
                if (!String.IsNullOrEmpty(thisMemberFinance.strCreditCardNumber))
                {
                    StringBuilder CreditCardNumber = new StringBuilder(Security.decode(thisMemberFinance.strCreditCardNumber)).Replace(" ", String.Empty);
                    txtNumber.Text = "*" + CreditCardNumber.Remove(0, CreditCardNumber.Length - 4).ToString();
                }
                txtExpiryMonth.Text = Security.decode(thisMemberFinance.strCreditExpMonth);
                txtExpiryYear.Text = Security.decode(thisMemberFinance.strCreditExpYear);
                txtCCV.Text = Security.decode(thisMemberFinance.strCreditCardCVV);
                txtNameonCard.Text = Security.decode(thisMemberFinance.strNameOnCreditCard);
                txtFinanceAdd1.Text = Security.decode(thisMemberFinance.strAddress1);
                txtFinanceAdd2.Text = Security.decode(thisMemberFinance.strAddress2);
                txtFinCity.Text = Security.decode(thisMemberFinance.strCity);
                txtFinZip.Text = thisMemberFinance.strZip;

                E164 objNumber = formatNumber(thisMemberFinance.strPhone);
                if (objNumber != null)
                    txtFinancePhone.Text = objNumber.formatNANPTn(E164.PATTERN_PARENS, false, false);

                if (thisMemberFinance != null && thisMemberFinance.iPlanType.HasValue)
                    ddlPlans.SelectedValue = thisMemberFinance.iPlanType.ToString();
            }
            lblNextBillingDate.Text = " Next Billing date :" + GetNextChargeDateForMember(securityCheck.iMemberId).ToShortDateString();
        }

        private DateTime GetNextChargeDateForMember(Int32 MemberID)
        {
            DateTime nextChargeDate = DateTime.UtcNow;
            List<MemberInvoiceBL> lstMemberInvoice = MemberInvoiceBL.getListByMemberId(securityCheck.iMemberId);
            if (lstMemberInvoice != null && lstMemberInvoice.Count > 0)
            {
                foreach (MemberInvoiceBL _objInvoice in lstMemberInvoice)
                {
                    if (_objInvoice.iProductId > 0 && _objInvoice.bStatus && _objInvoice.dtNextChargeDate.HasValue)
                    {
                        nextChargeDate = _objInvoice.dtNextChargeDate.Value;
                        break;
                    }
                }
            }

            foreach (MemberInvoiceBL _objInvoice in lstMemberInvoice)
            {
                if (_objInvoice.iProductId > 0 && _objInvoice.bStatus)
                {
                    if (DateTime.Compare(nextChargeDate, _objInvoice.dtNextChargeDate.Value) > 0)
                        nextChargeDate = _objInvoice.dtNextChargeDate.Value;
                }
            }
            return nextChargeDate;
        }

        private void Creditcardvisibility(Boolean flag)
        {
            ddlCreditCardName.Enabled = flag;
            ddlFinState.Enabled = flag;
            txtNumber.Enabled = flag;
            txtExpiryMonth.Enabled = flag;
            txtExpiryYear.Enabled = flag;
            txtCCV.Enabled = flag;
            txtNameonCard.Enabled = flag;
            txtFinanceAdd1.Enabled = flag;
            txtFinanceAdd2.Enabled = flag;
            txtFinCity.Enabled = flag;
            txtFinZip.Enabled = flag;
            txtFinancePhone.Enabled = flag;
        }

        private Boolean saveConditionInfo()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.saveConditionInfo()");
            Int32 iBrandProductId = 0;
            MembersBL thisMember = getMember();
            thisMember.Save();
            MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
            if (thisMemberFinance != null)
            {
                if (thisMemberFinance.iBrandProductId.HasValue)
                    iBrandProductId = thisMemberFinance.iBrandProductId.Value;
            }
            List<MemberConditionBL> lstMemberCondition = MemberConditionBL.getDataByMemberId(securityCheck.iMemberId);

            AppLogger.logInfo("End: MeridianEHealth.Personal.saveConditionInfo()");
            return true;
        }

        private Boolean saveSecurityInfo()
        {
            //MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
            MembersBL thisMember = securityCheck.objMember;
            if (thisMember != null)
            {
                Boolean isPasswordChanged = false;
                Boolean bFlag = false;
                Boolean bExistMember = false;
                if (!String.IsNullOrEmpty(ucNewPassword.ucPassword_Text))
                {
                    //Update user's password details
                    ChangePassword(thisMember, ref isPasswordChanged, ref bFlag, ref bExistMember);
                }

                //change security question and answers of the user
                ChageSecurityData(thisMember);
                thisMember.Save();
                if (bExistMember)
                {
                    if (bFlag)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return false;
        }

        private void ChangePassword(MembersBL thisMember, ref Boolean isPasswordChanged, ref Boolean bFlag, ref Boolean bExistMember)
        {
            if (securityCheck.iMemberId == securityCheck.iUserId && !String.IsNullOrEmpty(txtOldPassword.Text))
            {
                //set members passsword
                isPasswordChanged = ChangePassword(thisMember);
            }
            else
            {
                //MembersBL objMember = MembersBL.getByUserId(securityCheck.iUserId);
                MembersBL objMember = securityCheck.objUser;
                if ((objMember != null) && (objMember.iMemberType == 2 || objMember.iMemberType == 5 || objMember.iMemberType == 1))
                {
                    //set members passsword
                    isPasswordChanged = ChangePassword(thisMember);
                }
            }
            if (isPasswordChanged)
            {
                // change password of service data
                ChangeServicePassword(thisMember, out bFlag, out bExistMember);
            }
        }

        private Boolean ChangePassword(MembersBL thisMember)
        {
            thisMember.strPassword = Security.encode(ucNewPassword.ucPassword_Text);
            return true;
        }

        private void ChageSecurityData(MembersBL thisMember)
        {
            if (chkSecurityQst.Checked)
            {
                thisMember.strQuestion = ddlQuestion.SelectedValue.ToString();
                thisMember.strAnswer = Security.encode(txtAdditionalAnswer.Text);
            }
        }

        private Boolean ValidatePassword(String strFirstName, String strLastName, String strUserId)
        {
            Boolean bSuccess = true;
            try
            {
                String strErrors = string.Empty;
                // check new password and re-typed password is same of not
                if (ucNewPassword.ucPassword_Text != txtReTypeNewPassword.Text)
                {
                    strErrors += Constant.PASSWORD_NOT_MATCH;
                }
                // check user's password is valid or not
                ucNewPassword.checkPasswordValidation(strFirstName, strLastName, strUserId);
                if (ucNewPassword.Is_Error)
                {
                    strErrors += ucNewPassword.Error_Message;
                }
                // pop up a message if password is invalid
                if (!String.IsNullOrEmpty(strErrors))
                {
                    Master.AllowMessagePopupPostback = false;
                    Master.MessageTitle = Constant.ERROR;
                    Master.Message = strErrors;
                    Master.AllowMessagePopupPostback = false;
                    chkSecurityQst.Checked = false;
                    bSuccess = false;
                }
                return bSuccess;
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
                return false;
            }
        }

        private void ChangeServicePassword(MembersBL thisMember, out Boolean bFlag, out Boolean bExistMember)
        {
            //checking is user exist in service
            bFlag = false;
            bExistMember = false;
            try
            {
                HttpWebRequest requestExistsUser = GetWebRequest(String.Format(ExistsUser, thisMember.strUserId.ToLower() + "_" + objBrand.strMasterSponsorDomain));
                String jsonExistsUser = String.Empty;
                HttpWebResponse responseExistsUser = (HttpWebResponse)requestExistsUser.GetResponse();
                using (StreamReader sr = new StreamReader(responseExistsUser.GetResponseStream()))
                {
                    jsonExistsUser = sr.ReadToEnd();
                }
                String[] arrExistsUserResponse = getResponseArray(jsonExistsUser);
                foreach (String str in arrExistsUserResponse)
                {
                    if (str == "true")
                    {
                        //sending changed password to service
                        changeJsonPassword(thisMember, ref bFlag, ref bExistMember, objBrand);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
            }
        }

        private static void changeJsonPassword(MembersBL thisMember, ref Boolean bFlag, ref Boolean bExistMember, BrandBL objBrandBL)
        {
            bExistMember = true;
            String jsonSetPassword = String.Empty;
            HttpWebRequest requestSetPassword = GetWebRequest(String.Format(SetPassword, thisMember.strUserId.ToLower() + "_" + objBrandBL.strMasterSponsorDomain, Library.Security.decode(thisMember.strPassword)));

            HttpWebResponse responseSetPassword = (HttpWebResponse)requestSetPassword.GetResponse();
            using (StreamReader reader = new StreamReader(responseSetPassword.GetResponseStream()))
            {
                jsonSetPassword = reader.ReadToEnd();
            }
            String[] arrSetPassword = getResponseArray(jsonSetPassword);
            foreach (String s in arrSetPassword)
            {
                if (s == "success")
                {
                    bFlag = true;
                }
            }
        }

        private Boolean saveFinancialInfo()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.saveFinancialInfo()");

            MembersBL thisMember = getMember();
            AdditionalMembersBL thisAdditionalMembers = AdditionalMembersBL.getByMemberId(securityCheck.iMemberId);
            if (thisAdditionalMembers == null && thisMember.bIsDependent)
            {
                Master.ConfirmMessageTitle = Constant.INFO;
                Master.ConfirmMessage = "This action will change your account information and you have to relogin </br> Do you wish to continue?";
                return false;
            }
            MemberFinanceBL thisMemberfinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
            if (thisMemberfinance == null)
                thisMemberfinance = new MemberFinanceBL();

            thisMemberfinance.iMemberId = securityCheck.iMemberId;
            thisMemberfinance.iMembershipDesired = Convert.ToInt32(txtMemberShipDesired.Text);


            Int32 _iPlanType;
            if (Int32.TryParse(ddlPlans.SelectedValue, out _iPlanType))
            {
                if (thisMemberfinance.iPlanType != _iPlanType)
                {
                    thisMemberfinance.iPlanType = _iPlanType;
                    thisMemberfinance.Save();
                    UpdateMemberInvoiceForPlan();
                }
            }
            if (thisMember != null)
            {
                if (thisMember.iSponsorProgramId.HasValue)
                {

                    SponsorProgramBL thisSponsorProgram = SponsorProgramBL.getDataById(thisMember.iSponsorProgramId.Value);

                    if (thisSponsorProgram != null && thisSponsorProgram.iPayBy.HasValue)
                    {
                        if (thisSponsorProgram.iPayBy.Value == Convert.ToInt32(PAIDBY.INDIVIDUAL))
                        {
                            thisMemberfinance = saveCreditCardInfo(thisMemberfinance);
                        }
                    }
                }
                else
                {
                    thisMemberfinance = saveCreditCardInfo(thisMemberfinance);
                }
            }
            thisMemberfinance.Save();
            AppLogger.logInfo("End: MeridianEHealth.Personal.saveFinancialInfo()");
            return true;
        }

        private MemberFinanceBL saveCreditCardInfo(MemberFinanceBL thisMemberfinance)
        {
            Int32 _iCreditCardId;
            if (Int32.TryParse(ddlCreditCardName.SelectedValue, out _iCreditCardId))
            {
                thisMemberfinance.iCreditCardId = _iCreditCardId;
            }

            thisMemberfinance.strCreditCardNumber = Security.encode(txtNumber.Text.Trim());
            StringBuilder CreditCardNumber = new StringBuilder(txtNumber.Text);
            txtNumber.Text = "";
            if (CreditCardNumber.Length == 16)
                txtNumber.Text = "*" + CreditCardNumber.Remove(0, 12);
            else
                txtNumber.Text = "*" + CreditCardNumber.Remove(0, 10);
            //thisMemberfinance.strCreditCardNumber = Security.encode(HdfCreditCardNumber.Value.Trim());
            thisMemberfinance.strCreditExpMonth = Security.encode(txtExpiryMonth.Text.Trim());
            thisMemberfinance.strCreditExpYear = Security.encode(txtExpiryYear.Text.Trim());
            thisMemberfinance.strCreditCardCVV = Security.encode(txtCCV.Text.Trim());

            thisMemberfinance.strNameOnCreditCard = Security.encode(txtNameonCard.Text);
            thisMemberfinance.strAddress1 = Security.encode(txtFinanceAdd1.Text);
            thisMemberfinance.strAddress2 = Security.encode(txtFinanceAdd2.Text);
            thisMemberfinance.strCity = Security.encode(txtFinCity.Text);
            if (!String.IsNullOrEmpty(ddlFinState.Text))
                thisMemberfinance.strState = Security.encode(ddlFinState.SelectedValue);

            thisMemberfinance.strZip = txtFinZip.Text.Trim();

            thisMemberfinance.strPhone = objFinancePhone == null ? null : objFinancePhone.getDigits();
            txtFinancePhone.Text = objFinancePhone == null ? null : objFinancePhone.formatNANPTn(E164.PATTERN_PARENS, false, false);
            thisMemberfinance.bValidCreditCard = isValidCreditCard;

            return thisMemberfinance;
        }

        private MembersBL getMember()
        {
            MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
            if (thisMember == null)
                return null;

            thisMember.strFirstName = txtFirstName.Text.Trim();
            thisMember.strInitial = txtMiddle.Text.Trim();
            thisMember.strLastName = txtLastName.Text.Trim();
            thisMember.strEmail = txteMailAlias.Text.Trim();
            thisMember.dtDateOfBirth = dtTheBirthDate;
            thisMember.strPhoneticFirstName = txtPhoneticFirstName.Text.Trim();
            thisMember.strPhoneticLastName = txtPhoneticLastName.Text.Trim();
            Int32 iLanguageId = 0;
            if (!string.IsNullOrEmpty(ddlLanguage.SelectedItem.Text))
            {
                Int32.TryParse(Convert.ToString(ddlLanguage.SelectedValue), out iLanguageId);
            }
            else
            {
                Int32.TryParse(Convert.ToString(ddlLanguage.Items.FindByText(Constant.ENGLISH).Value), out iLanguageId);
            }
            thisMember.iLanguageid = iLanguageId;
            if (!String.IsNullOrEmpty(ddlGender.Text))
                thisMember.bGender = Convert.ToBoolean(ddlGender.SelectedValue);
            else
                thisMember.bGender = null;
            thisMember.iPayeeID = rblChangePayee.SelectedValue == Convert.ToInt32(Payee.Parent).ToString() ? (Nullable<Int32>)null : securityCheck.iMemberId;
            thisMember.bMemberList = chkIncludeName.Checked;
            return thisMember;
        }

        private Boolean validateHeader()
        {
            String strBirthDate = null;

            if ((txtMonth.Text != String.Empty && txtMonth.Text != String.Empty && txtYear.Text != String.Empty))
            {
                strBirthDate = txtMonth.Text + "/" + txtDate.Text + "/" + txtYear.Text;
                try
                {
                    dtTheBirthDate = Convert.ToDateTime(strBirthDate);
                }
                catch
                {
                    Master.Message = Constant.INVALID_BIRTH_DATE;
                    Master.MessageTitle = Constant.ERROR;
                    dtTheBirthDate = null;
                    txtYear.Text = String.Empty;
                    txtMonth.Text = String.Empty;
                    txtDate.Text = String.Empty;
                    return false;
                }
                if (dtTheBirthDate > DateTime.Today)
                {
                    Master.Message = Constant.INVALID_BIRTH_DATE;
                    Master.MessageTitle = Constant.ERROR;
                    txtYear.Text = String.Empty;
                    txtMonth.Text = String.Empty;
                    txtDate.Text = String.Empty;
                    return false;
                }
            }
            else
            {
                txtYear.Text = String.Empty;
                txtMonth.Text = String.Empty;
                txtDate.Text = String.Empty;
                dtTheBirthDate = null;
            }
            if (String.IsNullOrEmpty(txtFirstName.Text))
            {
                Master.Message = "Please enter first name";
                Master.MessageTitle = Constant.ERROR;
                return false;
            }
            return true;
        }

        private Boolean validateCellPhoneCapability()
        {
            String errorMessage = String.Empty;
            Boolean isSuccess = true;
            Int32 iDeleveryMethodID = 1;
            Boolean bIsCellPhoneOrTextMsgDest = false;
            Boolean bIsHomePhoneDest = false;
            Boolean bIsEmailDest = false;
            if (securityCheck.iMemberType != Convert.ToInt32(MemberType.Coach))
            {
                List<MemberDeliveryMethodBL> lstmemberDeliveryMethod = MemberDeliveryMethodBL.getDataByMemberId(securityCheck.iMemberId);
                if (lstmemberDeliveryMethod != null)
                {
                    foreach (MemberDeliveryMethodBL thisMemberDeliveryMethod in lstmemberDeliveryMethod)
                    {
                        iDeleveryMethodID = thisMemberDeliveryMethod.iDeliveryMethodId;
                        if (iDeleveryMethodID == Convert.ToInt32(deliveryMethod.cellphone) || iDeleveryMethodID == Convert.ToInt32(deliveryMethod.TextMsg))
                        {
                            bIsCellPhoneOrTextMsgDest = true;
                        }
                        else if (iDeleveryMethodID == Convert.ToInt32(deliveryMethod.homephone))
                        {
                            bIsHomePhoneDest = true;
                        }
                        else if (iDeleveryMethodID == Convert.ToInt32(deliveryMethod.email))
                        {
                            bIsEmailDest = true;
                        }
                    }
                }
            }

            MemberAlarmSettingBL thisMemberSetting = MemberAlarmSettingBL.getDataByMemberId(securityCheck.iMemberId);
            if (thisMemberSetting != null)
            {
                if (thisMemberSetting.iDeliverMethodId == Convert.ToInt32(deliveryMethod.cellphone) || thisMemberSetting.iDeliverMethodId == Convert.ToInt32(deliveryMethod.TextMsg))
                {
                    bIsCellPhoneOrTextMsgDest = true;
                }
                else if (thisMemberSetting.iDeliverMethodId == Convert.ToInt32(deliveryMethod.homephone))
                {
                    bIsHomePhoneDest = true;
                }
                else if (thisMemberSetting.iDeliverMethodId == Convert.ToInt32(deliveryMethod.email))
                {
                    bIsEmailDest = true;
                }
            }
            MemberDiaryEntriesSettingsBL thisDiaryEntrySettings = MemberDiaryEntriesSettingsBL.getDataByMemberId(securityCheck.iMemberId);
            if (thisDiaryEntrySettings != null)
            {

                if (thisDiaryEntrySettings.iDeliveryMethodId == Convert.ToInt32(deliveryMethod.TextMsg) || thisDiaryEntrySettings.iDeliveryMethodId == Convert.ToInt32(deliveryMethod.cellphone))
                {
                    bIsCellPhoneOrTextMsgDest = true;
                }
                else if (thisDiaryEntrySettings.iDeliveryMethodId == Convert.ToInt32(deliveryMethod.homephone))
                {
                    bIsHomePhoneDest = true;
                }
                else if (thisDiaryEntrySettings.iDeliveryMethodId == Convert.ToInt32(deliveryMethod.email))
                {
                    bIsEmailDest = true;
                }
            }

            if (isSuccess == false)
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = errorMessage;
            }
            return isSuccess;
        }

        private Boolean validateConditionInfo()
        {
            assignValidationGroup(Constant.VALIDATONGRP_CONDITION);

            if (!validateHeader())
                return false;
            return true;
        }

        private Boolean validateSecurityInfo()
        {
            MembersBL thisMember = securityCheck.objMember;//MembersBL.getByUserId(securityCheck.iMemberId);
            MembersBL thisParentLoginMember = securityCheck.objUser;//MembersBL.getByUserId(securityCheck.iUserId);
            if (thisMember != null)
            {
                if (!String.IsNullOrEmpty(txtOldPassword.Text) || !String.IsNullOrEmpty(ucNewPassword.ucPassword_Text) || !String.IsNullOrEmpty(txtReTypeNewPassword.Text))
                {
                    if ((thisMember.iMemberType != 2 || thisMember.iMemberType != 5) && (securityCheck.iMemberId == securityCheck.iUserId) || thisMember.iMemberType == 2 && (securityCheck.iMemberId == securityCheck.iUserId) || thisParentLoginMember.iMemberType == 1 && (securityCheck.iMemberId != securityCheck.iUserId))
                    {
                        if (String.IsNullOrEmpty(txtOldPassword.Text))
                        {
                            Master.Message = "Please enter old password";
                            Master.MessageTitle = Constant.ERROR;
                            return false;
                        }
                    }
                    if (String.IsNullOrEmpty(ucNewPassword.ucPassword_Text.Trim()))
                    {
                        Master.Message = "Please enter new password";
                        Master.MessageTitle = Constant.ERROR;
                        return false;
                    }

                    if (String.IsNullOrEmpty(txtReTypeNewPassword.Text.Trim()))
                    {
                        Master.Message = "Please retype new password";
                        Master.MessageTitle = Constant.ERROR;
                        return false;
                    }

                    if (!String.IsNullOrEmpty(txtOldPassword.Text) && Security.decode(thisMember.strPassword) != txtOldPassword.Text.Trim())
                    {
                        Master.Message = "Please enter correct old password";
                        Master.MessageTitle = Constant.ERROR;
                        return false;
                    }
                    // validate user's password on the basis of user details
                    if (!ValidatePassword(thisMember.strFirstName, thisMember.strLastName, thisMember.strUserId))
                    {
                        ClearPassword();
                        return false;
                    }
                }
                return true;
            }
            return false;

        }

        private void validateCreditCardInfo()
        {
            try
            {
                if (txtCCV.Enabled)
                {
                    objCreditCardValidation = new CreditCardValidation();
                    objCreditCardValidation.strCardSelected = ddlCreditCardName.SelectedItem.Text;
                    objCreditCardValidation.strCardNumber = txtNumber.Text;
                    objCreditCardValidation.strExpirationMonth = txtExpiryMonth.Text;
                    objCreditCardValidation.strExpirationYear = txtExpiryYear.Text;
                    objCreditCardValidation.strCCV = txtCCV.Text;
                    objCreditCardValidation.strCreditCardName = txtNameonCard.Text;
                    objCreditCardValidation.strAddress1 = txtFinanceAdd1.Text;
                    objCreditCardValidation.strAddress2 = txtFinanceAdd2.Text;
                    objCreditCardValidation.strCity = txtFinCity.Text;
                    objCreditCardValidation.strState = ddlFinState.SelectedValue;
                    objCreditCardValidation.strZip = txtFinZip.Text;
                    objCreditCardValidation.strPhone = txtFinancePhone.Text;

                    if (objCreditCardValidation.validateCreditCardInfo())
                    {
                        strCreditError = String.Empty;
                        isValidCreditCard = true;
                    }
                    else
                    {
                        strCreditError = "Your credit card information has not been validated.";
                        isValidCreditCard = false;
                        AppLogger.logInfo("End: MeridianEHealth.Personal.validateCreditCardInfo()");
                    }
                }
            }
            catch (Exception)
            {
                strCreditError = "Your credit card information has not been validated."; // Reason: <br/> " + ex.Message;
                isValidCreditCard = false;
                AppLogger.logInfo("End: MeridianEHealth.Personal.validateCreditCardInfo()");
            }
        }

        private Boolean validateFinanceInfo()
        {
            assignValidationGroup(Constant.VALIDATONGRP_FINANCE);

            MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);

            if (thisMember != null)
            {
                if (thisMember.iSponsorProgramId.HasValue)
                {
                    SponsorProgramBL thisSponsorProgram = SponsorProgramBL.getDataById(thisMember.iSponsorProgramId.Value);

                    if (thisSponsorProgram != null)
                    {
                        if (thisSponsorProgram.iPayBy.HasValue)
                        {
                            if (thisSponsorProgram.iPayBy.Value == Convert.ToInt32(PAIDBY.INDIVIDUAL))
                            {
                                return validateCreditCard();
                            }
                        }
                    }
                }
                else
                {
                    return validateCreditCard();
                }
            }

            return true;
        }

        private Boolean validateCreditCard()
        {

            MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);

            validateCreditCardInfo();

            if (!String.IsNullOrEmpty(strCreditError))
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = strCreditError;
                AppLogger.logInfo("End: MeridianEHealth.Signup.SignUp.validateFinanceInfo()");
                return false;
            }

            try
            {
                if (!String.IsNullOrEmpty(txtFinancePhone.Text))
                {
                    objFinancePhone = E164.validate(txtFinancePhone.Text.Trim());
                    objFinancePhone.formatNANPTn(E164.PATTERN_PARENS, false, false);
                }
            }
            catch
            {
                Master.MessageTitle = Constant.ERROR;
                Master.Message = "Invalid Finance phone number";
                return false;
            }

            return true;
        }

        private void fillPersonalInfo()
        {
            AppLogger.logInfo("Start: MeridianEHealth.Personal.fillPersonalInfo()");
            //PHN_PERFORMANCE
            MembersBL thisMember = securityCheck.objMember;// MembersBL.getByUserId(securityCheck.iMemberId);
            if (thisMember != null)
            {
                BillingControlShow();
                fillHeaderInfo(thisMember);
                fillContactInformationInfo(thisMember);
                fillConditionInfo(thisMember);
                fillfinanceInfo(thisMember);
            }

            AppLogger.logInfo("End: MeridianEHealth.Personal.fillPersonalInfo()");
        }

        private void activateAccount()
        {
            List<MembersBL> lstAdditionalMembers = MembersBL.getAdditionalMembers(securityCheck.iMemberId);
            if (lstAdditionalMembers != null && lstAdditionalMembers.Count > 0)
            {
                foreach (MembersBL objMember in lstAdditionalMembers)
                {
                    if (objMember.iActive.HasValue)
                    {
                        objMember.iActive = 1;
                        objMember.Save();
                    }
                }
            }
        }

        private void ClearSecurityDate()
        {
            chkSecurityQst.Checked = false;
            ClearPassword();
        }

        private void ClearPassword()
        {
            txtOldPassword.Text = String.Empty;
            ucNewPassword.ClearControl();
            txtReTypeNewPassword.Text = String.Empty;
        }

        private E164 formatNumber(String strNumber)
        {
            try
            {
                if (String.IsNullOrEmpty(strNumber))
                    return null;

                E164 objNumber = E164.validate(strNumber);
                return objNumber;
            }
            catch
            {

            }
            return null;
        }

        private Boolean CheckReadOnly()
        {
            MemberParentBL thisMemberParent = MemberParentBL.getByMemberAndParentId(securityCheck.iMemberId, securityCheck.iUserId);
            if (thisMemberParent != null)
            {
                if (thisMemberParent.bReadOnly)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void ClearCreditCard()
        {
            ddlFinState.SelectedValue = "";
            txtNumber.Text = String.Empty; ;
            txtExpiryMonth.Text = String.Empty; ;
            txtExpiryYear.Text = String.Empty; ;
            txtCCV.Text = String.Empty; ;
            txtNameonCard.Text = String.Empty; ;
            txtFinanceAdd1.Text = String.Empty; ;
            txtFinanceAdd2.Text = String.Empty; ;
            txtFinCity.Text = String.Empty; ;
            txtFinZip.Text = String.Empty; ;
            txtFinancePhone.Text = String.Empty;
        }

        protected void lnkGetStatement_Click(object sender, EventArgs e)
        {
            LoadPaymentAndChargeData();
        }

        protected void gvChargePayment_InitializeRow(object sender, RowEventArgs e)
        {
            try
            {
                if (e.Data as AccountStatementLinks != null)
                {
                    String strLink = (e.Data as AccountStatementLinks).InvoiceLink;

                    UltraGridCell Link = e.Row.Cells.FromKey("InvoiceLink");

                    if (Link != null)
                    {
                        Link.Text = "<a href='" + strLink + "' target='_blank'>Get Invoice</a>";
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }

        }

        private void LoadPaymentAndChargeData()
        {
            MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
            try
            {
                SipServiceSoapClient objSipServiceSoapClient = new SipServiceSoapClient();

                String errorMessage = String.Empty;
                String strExtBillingAccountId = thisMember.strExtBillingAccountId;
                DateTime dtStartFrom = Convert.ToDateTime(wdcStartDate.Value);
                DateTime dtEnd = Convert.ToDateTime(wdcEndDate.Value);

                AccountStatementLinks[] arrAccountStatementLinks = objSipServiceSoapClient.GetStatementLinks(strExtBillingAccountId, dtStartFrom, dtEnd, ref errorMessage);
                
                if (arrAccountStatementLinks != null)
                {
                    gvChargePayment.DataSource = arrAccountStatementLinks;
                    gvChargePayment.DataBind();
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
                Utils.sendExceptionMail(ex);
            }

        }

        private void setOneTimeCharge(List<GetChargeAndPaymentByMemberIdBL> _lstPayment, List<GetChargeAndPaymentByMemberIdBL> _lstGetChargeAndPaymentByMemberIdBL)
        {
            foreach (GetChargeAndPaymentByMemberIdBL objGetChargeAndPaymentByMemberIdBL in _lstPayment)
            {
                if (objGetChargeAndPaymentByMemberIdBL.iHistoryId.HasValue)
                {
                    InvoiceBL objInvoiceBL = InvoiceBL.getDatabyId(objGetChargeAndPaymentByMemberIdBL.iHistoryId.Value);
                    MemberFinanceBL _objMemberFinanceBL = MemberFinanceBL.getByMemberId(securityCheck.iMemberId);
                    if (objInvoiceBL != null)
                    {
                        MemberInvoiceBL objMemberInvoiceBL = MemberInvoiceBL.getDataByInvoiceId(objInvoiceBL.iInvoiceId);
                        if (objMemberInvoiceBL != null && ((objMemberInvoiceBL.dContractPrice > 0
                            && objMemberInvoiceBL.dContractPrice != objInvoiceBL.dCurrentCharges) || objMemberInvoiceBL.dCurrentRecurringCharge != objInvoiceBL.dCurrentCharges)
                            && objMemberInvoiceBL.iProductId != 0 && objMemberInvoiceBL.dOneTimePrice > 0)
                        {
                            Decimal dDiscountOneTime = 0;
                            Decimal dExtended;
                            Decimal dOneTime;
                            String strDescriptionForOneTime = String.Empty;
                            GetChargeAndPaymentByMemberIdBL thisGetChargeAndPaymentByMemberIdBL = new GetChargeAndPaymentByMemberIdBL();
                            if (objInvoiceBL.dDiscount.HasValue && objInvoiceBL.dDiscount > 0)
                            {
                                dDiscountOneTime = objMemberInvoiceBL.dOneTimePrice - (objMemberInvoiceBL.dOneTimePrice * (objInvoiceBL.dDiscount.Value / 100));
                                if (_objMemberFinanceBL != null && !String.IsNullOrEmpty(_objMemberFinanceBL.strState) && Security.decode(_objMemberFinanceBL.strState) == "FL")
                                {
                                    Decimal PercentTaxes;
                                    if (Decimal.TryParse("0.07", out PercentTaxes))
                                    {
                                        dOneTime = dDiscountOneTime * PercentTaxes;
                                        dExtended = objGetChargeAndPaymentByMemberIdBL.dExtendedPrice.Value - Decimal.Round(dDiscountOneTime, 2);
                                        objGetChargeAndPaymentByMemberIdBL.dExtendedPrice = dExtended - dOneTime;
                                        thisGetChargeAndPaymentByMemberIdBL.dExtendedPrice = Decimal.Round(dDiscountOneTime, 2) + Decimal.Round(dOneTime, 2);
                                    }
                                }
                                else
                                {
                                    objGetChargeAndPaymentByMemberIdBL.dExtendedPrice = objGetChargeAndPaymentByMemberIdBL.dExtendedPrice - Decimal.Round(dDiscountOneTime, 2);
                                    thisGetChargeAndPaymentByMemberIdBL.dExtendedPrice = Decimal.Round(dDiscountOneTime, 2);
                                }
                            }
                            else
                            {
                                if (_objMemberFinanceBL != null && !String.IsNullOrEmpty(_objMemberFinanceBL.strState) && Security.decode(_objMemberFinanceBL.strState) == "FL")
                                {
                                    Decimal PercentTaxes;
                                    if (Decimal.TryParse("0.07", out PercentTaxes))
                                    {
                                        dOneTime = objMemberInvoiceBL.dOneTimePrice * PercentTaxes;
                                        dExtended = objGetChargeAndPaymentByMemberIdBL.dExtendedPrice.Value - objMemberInvoiceBL.dOneTimePrice;
                                        objGetChargeAndPaymentByMemberIdBL.dExtendedPrice = dExtended - dOneTime;
                                        thisGetChargeAndPaymentByMemberIdBL.dExtendedPrice = objMemberInvoiceBL.dOneTimePrice + Decimal.Round(dOneTime, 2);
                                    }
                                }
                                else
                                {
                                    objGetChargeAndPaymentByMemberIdBL.dExtendedPrice = objGetChargeAndPaymentByMemberIdBL.dExtendedPrice - objMemberInvoiceBL.dOneTimePrice;
                                    thisGetChargeAndPaymentByMemberIdBL.dExtendedPrice = objMemberInvoiceBL.dOneTimePrice;
                                }

                            }
                            thisGetChargeAndPaymentByMemberIdBL.iHistoryId = objGetChargeAndPaymentByMemberIdBL.iHistoryId;
                            thisGetChargeAndPaymentByMemberIdBL.iMemberId = objGetChargeAndPaymentByMemberIdBL.iMemberId;
                            thisGetChargeAndPaymentByMemberIdBL.iMemberInvoiceId = objGetChargeAndPaymentByMemberIdBL.iMemberInvoiceId;
                            thisGetChargeAndPaymentByMemberIdBL.iTransctionType = objGetChargeAndPaymentByMemberIdBL.iTransctionType;
                            if (objGetChargeAndPaymentByMemberIdBL.strDescription.Contains("-"))
                                strDescriptionForOneTime = objGetChargeAndPaymentByMemberIdBL.strDescription.Substring(objGetChargeAndPaymentByMemberIdBL.strDescription.LastIndexOf("-"));
                            if (strDescriptionForOneTime.Trim() == "-")
                                strDescriptionForOneTime = objGetChargeAndPaymentByMemberIdBL.strDescription.Substring(objGetChargeAndPaymentByMemberIdBL.strDescription.IndexOf("-"));
                            if (!String.IsNullOrEmpty(strDescriptionForOneTime))
                                thisGetChargeAndPaymentByMemberIdBL.strDescription = objGetChargeAndPaymentByMemberIdBL.strDescription.Replace(strDescriptionForOneTime, String.Empty) + "- One Time";
                            else
                                thisGetChargeAndPaymentByMemberIdBL.strDescription = objGetChargeAndPaymentByMemberIdBL.strDescription + " - One Time";
                            thisGetChargeAndPaymentByMemberIdBL.dtCreateDate = objGetChargeAndPaymentByMemberIdBL.dtCreateDate;
                            thisGetChargeAndPaymentByMemberIdBL.bIsOneTime = true;
                            _lstGetChargeAndPaymentByMemberIdBL.Add(thisGetChargeAndPaymentByMemberIdBL);
                        }
                    }
                }
            }
        }

        private bool ShowCreditCardInfo(MembersBL thisMember)
        {
            bool bShow = false;
            if (thisMember != null)
            {
                MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(thisMember.iId);
                // parent member
                if (!thisMember.bIsDependent)
                {
                    List<AdditionalMembersBL> lstAdditionalMembers = AdditionalMembersBL.getDataByMemberId(securityCheck.iMemberId);

                    // Sponsored member
                    if (thisMember.iSponsorProgramId.HasValue)
                    {
                        SponsorProgramBL thisSponsorProgram = SponsorProgramBL.getDataById(thisMember.iSponsorProgramId.Value);
                        if (thisSponsorProgram != null && thisSponsorProgram.iPayBy.HasValue)
                        {
                            if (lstAdditionalMembers != null)
                            {
                                if (thisSponsorProgram.iPayBy.Value == Convert.ToInt32(PAIDBY.INDIVIDUAL) || (lstAdditionalMembers.Count > thisSponsorProgram.iDependentMembers))
                                {
                                    if (thisMemberFinance != null && thisMemberFinance.iBrandProductId.HasValue)
                                    {
                                        bShow = true;
                                    }
                                }


                            }
                            else
                            {
                                if (thisSponsorProgram.iPayBy.Value == Convert.ToInt32(PAIDBY.INDIVIDUAL))
                                {
                                    if (thisMemberFinance != null && thisMemberFinance.iBrandProductId.HasValue)
                                    {
                                        bShow = true;
                                    }
                                }
                            }
                        }
                    }
                    else  // enrolled member
                    {
                        if (thisMemberFinance != null && thisMemberFinance.iBrandProductId.HasValue)
                        {
                            bShow = true;
                        }
                    }
                }
                else    // additional member
                {
                    AdditionalMembersBL thisAdditionalMembers = AdditionalMembersBL.getByMemberId(thisMember.iId);
                    {
                        if (thisAdditionalMembers != null)
                        {
                            bShow = false;
                        }
                        else
                        {
                            bShow = true;
                        }
                    }
                }
            }
            return bShow;
        }

        private void BindUsageHistory()
        {
            List<AutomatedDailyReportBL> lstAutomatedDailyReportBL = AutomatedDailyReportBL.getData(securityCheck.iMemberId);
            if (lstAutomatedDailyReportBL != null)
            {
                gvUsageHistory.DataSource = lstAutomatedDailyReportBL;
                gvUsageHistory.DataBind();
            }
        }

        private static HttpWebRequest GetWebRequest(string formattedUri)
        {
            //Create the request’s URI
            Uri serviceUri = new Uri(formattedUri, UriKind.Absolute);
            //Return the HttpWebRequest
            return (HttpWebRequest)System.Net.WebRequest.Create(serviceUri);
        }

        private static String[] getResponseArray(String strInput)
        {
            String[] strArray;
            strArray = strInput.Split(new char[] { ';' });
            return strArray;
        }

        private Boolean SendMail(MailType eMailType)
        {
            try
            {
                AppLogger.logInfo("Start: MeridianEHealth.Sponsor.Applications.SendMail()");

                Int32 _iSenderMemberId = securityCheck.iMemberId;
                SmtpClient _objSmtpClient;
                Email _objEmail;

                MembersBL thisSenderMember = MembersBL.getByUserId(_iSenderMemberId);
                SponsorBL thisMemberSponsor = null;
                BrandEmailTemplateBL thisBrandEmailTemplate = null;
                thisBrandEmailTemplate = BrandEmailTemplateBL.getDataBySponsorIdAndEmailTemplateType(Convert.ToInt32(EmailTemplateType.Accept), thisSenderMember.iSponserId.Value);

                if (thisBrandEmailTemplate == null)
                    thisBrandEmailTemplate = BrandEmailTemplateBL.getMSEmailTemplateByEmailTemplateType(Convert.ToInt32(EmailTemplateType.Accept), BrandBL.objBrand.iId);

                if (thisSenderMember.iSponserId.HasValue)
                    thisMemberSponsor = SponsorBL.getDataById(thisSenderMember.iSponserId.Value);
                if (thisSenderMember != null)
                {

                    MembersBL thisMember = MembersBL.getByUserId(securityCheck.iMemberId);
                    if (thisMember != null)
                    {
                        _objSmtpClient = new SmtpClient();
                        _objEmail = new Email();
                        _objEmail.To = thisMember.strEmail;
                        String fromEmail = ConfigurationManager.AppSettings["SupportEmailId"];

                        if (thisMemberSponsor != null && !String.IsNullOrEmpty(thisMemberSponsor.strSponsorHelpEmail))
                        {
                            fromEmail = thisMemberSponsor.strSponsorHelpEmail;
                        }
                        else if (!String.IsNullOrEmpty(BrandBL.objBrand.strHelpEmail))
                        {
                            fromEmail = BrandBL.objBrand.strHelpEmail;
                        }
                        _objEmail.From = fromEmail;
                        _objEmail.Subject = Utils.getEmailTemplate(thisBrandEmailTemplate.strEmailTemplateSubject, thisMember);
                        _objEmail.Body = Utils.getEmailTemplate(thisBrandEmailTemplate.strEmailTemplateMessage, thisMember);
                        _objEmail.Body = Utils.unusedEmailItems(_objEmail.Body);
                        _objEmail.SendEmail(_objSmtpClient);

                        MemberApplicationActionsBL thisMemberApplication = MemberApplicationActionsBL.getDataByMemberID(securityCheck.iMemberId);

                        switch (eMailType)
                        {
                            case MailType.Approve:
                                thisMember.iActive = Convert.ToInt32(MemberStatus.Active);
                                MemberFinanceBL thisMemberFinance = MemberFinanceBL.getByMemberId(thisMember.iId);
                                if (thisMember.iSponsorProgramId.HasValue)
                                {
                                    if (thisMemberFinance != null)
                                        thisMember.dtExpirationDate = MembersBL.getExpirationDate(thisMember.iSponsorProgramId, thisMemberFinance.iPlanType, thisMember.iActive);
                                    else
                                        thisMember.dtExpirationDate = MembersBL.getExpirationDate(thisMember.iSponsorProgramId, null, thisMember.iActive);
                                }
                                if (thisMemberApplication != null)
                                {
                                    thisMemberApplication.iAction = Convert.ToInt32(ApplicationStatus.Approved);
                                    thisMemberApplication.strNote = null;
                                    thisMemberApplication.Save();
                                }
                                break;
                        }

                        thisMember.Save();


                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    AppLogger.logInfo("End: MeridianEHealth.Sponsor.Applications.SendMail()");
                    return false;
                }
                AppLogger.logInfo("End: MeridianEHealth.Sponsor.Applications.SendMail()");
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }
            return true;
        }

        private void AssignValues(int iPhone, E164 objPhone)
        {
            switch (iPhone)
            {
                case 1:
                    objCellPhone = objPhone;
                    break;
                case 2:
                    objHomePhone = objPhone;
                    break;
                case 3:
                    objWorkPhone = objPhone;
                    break;
                case 4:
                    objWorkCellPhone = objPhone;
                    break;
            }
        }

        #endregion

        #region  "Plan Type Change"
        private void UpdateMemberInvoiceForPlan()
        {
            try
            {
                PaymentProcess _objPayment = new PaymentProcess();
                _objPayment.UpdatePaymentProcessForMember(securityCheck.iMemberId);
            }
            catch (Exception ex)
            {
                Utils.sendExceptionMail(ex);
                ExceptionPolicy.HandleException(ex, Constant.GLOBAL_EXCEPTION_POLICY);
            }

        }
        #endregion "Plan Type Change"


    }
}
