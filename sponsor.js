/**************************Class level Variable***************************************/
var productId = 0;
var selectedProductPrize = 0;
var selectedProdcutDiscount = '0';
var extraPersonalNumber = 0;
var totalPrice = 0;
var limitedNumberTypeId = 0;
var authorizationNumber = null;
var sponsorName = '';
var lstPhoneNumber = null;
var isLocalNumberOnOrder = false;
var isTollFreeNumberOnOrder = false;
var iMemberStatus = 1;
var extBillingAccountId = 0;
var isBillingPageVisible = false;
var isValidCreditCard = false;
var isBillingPriceZero = false;
var iVBOSPackageProductId = 0;
var qsSponsorName = null;
var qsAuthCode = null;

var reqDataCreditCard = null;
var reqDataACH = null
var reqDataBillingAddress = null;
var reqDataCustomerInfo = null;
var iMemberId = 0;
var isBillingSectionHidden = false;
var iPayBy = 0;
var ExtraPersonalNumberCount = 0;
var isTollFreeNumberRequested = false;
var isLocalNumberRequested = false;
var isLocalNumberReturned = false;
var isTollFreeNumberReturned = false;
var isFromAnyWhere;
var isFromAnyWhereReturned = false;
var isFromGeographicReturned = false;
var strPhoneNumber;
/************************************************************************************/
function GetParameterValues(param) {
    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < url.length; i++) {
        var urlparam = url[i].split('=');
        if (urlparam[0] == param) {
            return urlparam[1];
        }
    }
}

$(document).ready(function () {
    var sponsorTemp = GetParameterValues('sponsor');
    var authCodeTemp = GetParameterValues('authcode');
    if (sponsorTemp != '' && sponsorTemp != null && sponsorTemp != undefined) {
        qsSponsorName = sponsorTemp;
        qsAuthCode = authCodeTemp;
    }

    $("#btnBack").hide();
    $("#btnNext").hide();
    $("#localNumberCollapse").hide();
    LoadSponsor();

    $("#btnNext").click(function () {
        $(this).blur();
        BtnNextClick();
    });

    $("#btnBack").click(function () {
        $(this).blur();
        BtnBackClick();
    });

    $("#rblCreditCard").click(function () {
        var chk = $(this).attr('checked');
        ShowCreditCardDetails(true);
    });

    $("#rblACH").click(function () {
        var chk = $(this).attr('checked');
        ShowCreditCardDetails(false);
    });

    $("#rblUnitedStatesAddress").click(function () {
        var chk = $(this).attr('checked');
        ShowUnitedStateAddress(true);
    });

    $("#rblInternationalAddress").click(function () {
        var chk = $(this).attr('checked');
        ShowUnitedStateAddress(false);
    });

    $(".numeric").numeric();

    $("#rblImmediatelyNumber").click(function () {
        var chk = $(this).attr('checked');
        $("#txtPhoneNumber").attr("disabled", "disabled");
    });

    $("#rblGeographicLocation").click(function () {
        var chk = $(this).attr('checked');
        $("#txtPhoneNumber").removeAttr("disabled");
        $("#txtPhoneNumber").focus();
    });


    $('#txtCardNumber').formattercc({ cctypefield: 'typeCard' });

    //$('#txtCardNumber').keydown(function () {
    //    $(this).val(function (i, v) {
    //        var v = v.replace(/[^\d]/g, '').match(/.{1,4}/g);
    //        return v ? v.join(' ') : ' ';
    //    });
    //});

    //$('#txtCardNumber').keyup(function () {
    //    $(this).val(function (i, v) {
    //        var v = v.replace(/[^\d]/g, '').match(/.{1,4}/g);
    //        return v ? v.join(' ') : ' ';
    //    });
    //});

    $('#chkLocalNumber').click(function () {
        if ($(this).is(':checked') ) {
            $("#localNumberCollapse").show();
        }
        else { $("#localNumberCollapse").hide(); }

        if (limitedNumberTypeId == 3 && $(this).is(':checked') && $('#chkTollFreeNumber').is(':checked')) {
            $('#lblExtraNumberCostMessage').show();
        } else {
            $('#lblExtraNumberCostMessage').hide();
        }
    });

    $('#chkTollFreeNumber').click(function () {
        if (limitedNumberTypeId == 3 &&  $(this).is(':checked') && $('#chkLocalNumber').is(':checked')) {
            $('#lblExtraNumberCostMessage').show();
        } else {
            $('#lblExtraNumberCostMessage').hide();
        }
    });
    
    
    LoadYears();
    SetTOSLink();

    // ^(\$|)([1-9]\d{0,2}(\,\d{3})*|([1-9]\d*))(\.\d{2})?$

    /***********Branding related *******************/
    var borderColor = $(".TextBoxX").css("border-bottom-color");
    $(".form-control").css("border-color", borderColor);
    /***********************************************/

});

/*-------------------------------------------------Load Events-------------------------------------------------------------------------------*/
function ShowLoadingPanel() {
    $("#loadingDiv").show();
    var content = $("#wrapper");
    content.addClass('content-loading');
}
function hideLoadingPanel() {
    var content = $("#wrapper");
    content.removeClass('content-loading');
    $("#loadingDiv").hide();
}


/*LoadMonths*/
function LoadMonths() {

    var ddlMonth = $('#ddlMonth');

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetAllMonths",
        data: '',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            ddlMonth.html('');
            $(response.d).each(function (index) {
                var OptionValue = index;
                var OptionText = response.d[index];
                if (OptionText != '') {
                    // Create an Option for DropDownList.
                    var option = $("<option>" + OptionText + "</option>");
                    option.attr("value", OptionValue);

                    ddlMonth.append(option);
                }
            });
            hideLoadingPanel();
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

/*LoadYears*/
function LoadYears() {
    var ddlYear = $('#ddlYear');

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetExpiryYears",
        data: '{MaxYear: 5}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            //ShowLoadingPanel();
        },
        success: function (response) {
            ddlYear.html('');
            $(response.d).each(function (index) {
                var OptionValue = response.d[index];
                var OptionText = response.d[index];
                if (OptionText != '') {
                    // Create an Option for DropDownList.
                    var option = $("<option>" + OptionText + "</option>");
                    option.attr("value", OptionValue);

                    ddlYear.append(option);
                }
            });
            ///hideLoadingPanel();
        },
        failure: function (response) {
            // hideLoadingPanel();
            alert(response.d);
        }
    });
}

function SetTOSLink() {
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/SetTOSLink",
        data: '',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            //ShowLoadingPanel();
        },
        success: function (response) {
            if (response.d.IsSuccess == true) {
                $("#lnkToS").attr("href", response.d.TOSLink);
            }
        },
        failure: function (response) {
            // hideLoadingPanel();
        }
    });
}

/*load language*/
function LoadLanguages() {
    var ddlPreferredLanguage = $('#ddlPreferredLanguage');

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetLanguages",
        data: '',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            //ShowLoadingPanel();
        },
        success: function (response) {
            ddlPreferredLanguage.html('');
            $(response.d).each(function (index) {
                var OptionValue = $(this)[0].iId;
                var OptionText = $(this)[0].strLanguageName;

                if (OptionText != '' && OptionValue != 0) {
                    // Create an Option for DropDownList.
                    var option = $("<option>" + OptionText + "</option>");
                    option.attr("value", OptionValue);

                    ddlPreferredLanguage.append(option);
                }
            });
            //hideLoadingPanel();
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

//LoadSponsor
function LoadSponsor() {
    var ddlSponsor = $('#ddlSponsor');
    var selectedValue = -1;
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetSponsor",
        data: '',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            ddlSponsor.html('');
            $(response.d).each(function () {
                var OptionValue = $(this)[0].iId;
                var OptionText = $(this)[0].strSponsor;

                // Create an Option for DropDownList.
                var option = $("<option>" + OptionText + "</option>");
                option.attr("value", OptionValue);

                ddlSponsor.append(option);

                if (OptionText == decodeURIComponent(qsSponsorName)) {
                    selectedValue = OptionValue;
                }
            });

            if (qsSponsorName != null) {
                if (selectedValue == -1) {
                    hideLoadingPanel();
                    $("#lblGlobalError").show().html("Enter valid Sponsor/Group name.");
                }
                else {
                    $("#ddlSponsor").val(selectedValue);
                    $("#txtAuthCode").val(qsAuthCode);
                    ValidateSponsorSelect();
                    ValidateAuthorizationNumber();
                }
            }
            else {
                $("#tab-sponsorSelection").show();
                $("#btnNext").show();
                hideLoadingPanel();
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

//LoadSponsorProduct
function LoadSponsorProduct() {
    var ddlProduct = $('#ddlProduct');
    var sponsorProgramId = $("#hdnLblSponsorProgramId").val();
    var iSponsorId = $("#ddlSponsor option:selected").val();
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetBrandPrduct",
        data: '{sponsorProgramId : "' + sponsorProgramId + '",iSponsorId : "' + iSponsorId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            ddlProduct.html('');
            var count = response.d.length;

            $(response.d).each(function () {
                var OptionValue = $(this)[0].iId;
                var OptionText = $(this)[0].strProductName;

                // Create an Option for DropDownList.
                var option = $("<option>" + OptionText + "</option>");
                option.attr("value", OptionValue);

                ddlProduct.append(option);
            });
            if (count >= 1) {
                SelectSponsorProduct();
            }
            else {
                hideLoadingPanel();
                $("#lblBillingPeriod").html("");
                $("#lblProductDesc").html("")
                $("#lblProductPrice").html("0");
                $("#lblSavings").html("0");
                $("#lblYourPrice").html("0");
            }
            LoadLanguages();
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

function SelectSponsorProduct() {
    productId = $("#ddlProduct option:selected").val();
    var authCode = $("#txtAuthCode").val();

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetPrductDetails",
        data: '{productId: "' + productId + '", authCode : "' + authCode + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            if (response.d.Discription != null) {
                $("#lblProductDesc").html(response.d.Discription);
                $("#lblProductDesc").show();
            }
            else {
                $("#lblProductDesc").html("");
            }

            if (response.d.BillingPeriod == "Monthly") {
                $("#lblBillingPeriod").html("Monthly");

                $("#lblProductPrice").html(response.d.ListPrice);
                $("#lblSavings").html(response.d.Discount);
                $("#lblYourPrice").html(response.d.CalculatePrice);
                selectedProductPrize = response.d.ListPrice;
                selectedProdcutDiscount = response.d.Discount;
            }
            else {
                $("#lblBillingPeriod").html("-");

                $("#lblProductPrice").html("0");
                $("#lblSavings").html("0");
                $("#lblYourPrice").html("0");
            }
            if ($("#lblProductDesc").text() == "") {
                $("#lblProductDesc").hide();
            }
            else {
                $("#lblProductDesc").show();
            }

            limitedNumberTypeId = response.d.LimitedNumberTypeId;
            iVBOSPackageProductId = response.d.VBOSPackageId;

            hideLoadingPanel();

            if (IsInvalidBillingPeriod()) {
                $("#lblInvalidBillingPeriodMsg").show();
                return;
            }
            else {
                $("#lblInvalidBillingPeriodMsg").hide();
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

//Hide or Show Billing Screen Input fields according to SponsorPaymentApproch (i.e. SponsorAlways, IndividualAlways)
function SponsorPaymentApproch() {
    var iSponsorProgramId = $("#hdnLblSponsorProgramId").val();
    var iSponsorId = $("#ddlSponsor option:selected").val()
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/CheckSponsorPaymentApproch",
        data: '{strSponsorId: "' + iSponsorId + '",strSponsorProgramId: "' + iSponsorProgramId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            if (response.d.IsSuccess == true) {

                if (response.d.PaymentApproch == 0) { //SponsorPay then PaymentApproach = 0 
                    $("#BillingSection").hide();
                    isBillingSectionHidden = true;
                    //IsSponsorWithVBOSAccNo();
                    hideLoadingPanel();
                    BtnNextClick(true);
                }
                else {
                    $("#BillingSection").show();
                    isBillingSectionHidden = false;
                    iPayBy = 1;
                    hideLoadingPanel();
                    BtnNextClick(true);
                }
            }
            else {
                hideLoadingPanel();
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

function IsSponsorWithVBOSAccNo() {
    var sponsorId = $("#ddlSponsor option:selected").val();

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/IsSponsorWithVBOSAccNo",
        data: '{strSponsorId: "' + sponsorId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {

        },
        success: function (response) {
            hideLoadingPanel();
            $("#lblSponsorError").remove();
            $("#ddlSponsor").removeClass("invalidSponsorError");
            if (!response.d.IsSuccess) {
                $("#ddlSponsor").addClass("invalidSponsorError");
                var isInValid = $("#ddlSponsor").hasClass("invalidSponsorError");
                if (isInValid) {
                    $("<label id='lblSponsorError' for='ddlSponsor' class='errorLable' generated='true'>Selected Sponosor has no VBOS Account Number.</label>").insertBefore(".invalidSponsorError");
                    if (qsSponsorName != null) {
                        $("#lblGlobalError").show().html("Sponosor has no VBOS Account Number.");
                    }
                }
                return;
            }
            BtnNextClick(true);
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

/*-------------------------------------------------End Load Events-------------------------------------------------------------------------------*/
/*-------------------------------------------------Button events-------------------------------------------------------------------------------*/
function BtnNextClick(bSkipValidation) {
    $("#lblSponsorError").remove();
    $("#ddlSponsor").removeClass("invalidSponsorError");
    var currentTab = $(".tab-current");
    var validateTabIndex = currentTab.attr("data-index");
    if (bSkipValidation != true) {
        if (ValidateTabs(validateTabIndex) == false) {
            return;
        }
        if (validateTabIndex == 1) {

            SponsorPaymentApproch();
            return;
        }
    }

    var nextTab = $(".tab-current").next();
    var nextTabIndex = nextTab.attr("data-index");

    if (nextTabIndex == 3 && limitedNumberTypeId == null) {
        nextTab = $(".tab-current").next().next();
        extraPersonalNumber = 0;
        LoadBillingInformationTab();
    }
    if (nextTabIndex == 3 && IsInvalidBillingPeriod()) {
        return;
    }
    var nextTabIndex = nextTab.attr("data-index");
    isBillingPriceZero = false;
    price = (Number(selectedProductPrize) + Number(extraPersonalNumber) - Number(selectedProdcutDiscount)).toFixed(2);
    if (price == "0.00") {
        isBillingPriceZero = true;
    }
    if (nextTabIndex == 4 && isBillingPriceZero == true) {
        if (limitedNumberTypeId == null) {
            nextTab = $(".tab-current").next().next().next();
            LoadBillingInformationTab();
        } else {
            nextTab = $(".tab-current").next().next();
            LoadBillingInformationTab();
        }
    }

    nextTab.show();
    currentTab.hide();

    currentTab.removeClass("tab-current");
    nextTab.addClass("tab-current");
    currentTabIndex = nextTab.attr("data-index");

    LoadTab(currentTabIndex);
}

function BtnBackClick() {
    var currentTab = $(".tab-current");
    var previousTab = $(".tab-current").prev();
    var preTabIndex = previousTab.attr("data-index");
    if (preTabIndex == 3 && limitedNumberTypeId == null) {
        previousTab = $(".tab-current").prev().prev();
    }
    else {
        previousTab = $(".tab-current").prev();
    }
    isBillingPriceZero = false;
    price = (Number(selectedProductPrize) + Number(extraPersonalNumber) - Number(selectedProdcutDiscount)).toFixed(2);
    if (price == "0.00") {
        isBillingPriceZero = true;
    }
    if (preTabIndex == 4 && isBillingPriceZero == true) {
        if (limitedNumberTypeId == null) {
            previousTab = $(".tab-current").prev().prev().prev();
            LoadBillingInformationTab();
        } else {
            previousTab = $(".tab-current").prev().prev();
            LoadBillingInformationTab();
        }
    }

    currentTab.hide();
    previousTab.show();

    currentTab.removeClass("tab-current");
    previousTab.addClass("tab-current");
    currentTabIndex = previousTab.attr("data-index");

    $("#txtAuthCode").removeClass("invalidAuthCode");
    if (currentTabIndex == 1) {
        ReloadMasterImages(0);
    }
    LoadTab(currentTabIndex);
}

/*-------------------------------------------------End Button events-------------------------------------------------------------------------------*/
/*-------------------------------------------------LoadTabs-------------------------------------------------------------------------------*/
function ValidateTabs(currentTabIndex) {
    if (currentTabIndex == 0) {
        //LoadRequestDemoTab();
    }
    else if (currentTabIndex == 1) {
        ValidateSponsorSelect();
        if ($("#txtAuthCode").val().trim() != "") {
            ValidateAuthorizationNumber();
            return false;
        }
        else {
            if (authorizationNumber != $("#txtAuthCode").val().trim()) {
                authorizationNumber = '';
                productId = 0;
            }
        }

        $("#lblAuthCodeError").remove();
        $("#hdnLblSponsorProgramId").val(0);
        //return ValidateSponsorSelect();
        return true;
    }
    else if (currentTabIndex == 2) {
        if (ValidateCustomerDetails()) {
            $("#txtPhoneNumber").val($("#txtMobileNumber").val());
            if (selectedProductPrize > 0) {
                //GetPhoneNumber();
                return true;
            }
            else {
                return true;
            }
        }
        return false;
    }
    else if (currentTabIndex == 3) {
        ReleaseNumberAndGetPhoneNumber();
        return false;
    }
    else if (currentTabIndex == 4) {
        if (!isBillingSectionHidden) {
            ValidateBillingInfo();
        } else { return true; }
        return false;
    }
    else if (currentTabIndex == 5) {
        var retTnS = ValidateTermsAndService();
        if (retTnS) {
            UserSignUp();
        }
        return false;
    }
}

function LoadTab(currentTabIndex) {
    if (currentTabIndex == 2 && qsSponsorName != null) {
        $("#btnBack").hide();
    }
    else {
        $("#btnBack").show();
    }
    $("#btnNext").show();
    if (currentTabIndex == 0) {
        LoadRequestDemoTab();
    }
    else if (currentTabIndex == 1) {
        $("#btnBack").hide();
        LoadSponosrSelectTab();
    }
    else if (currentTabIndex == 2) {
        var iSponsor_id = $("#ddlSponsor option:selected").val();
        ReloadMasterImages(iSponsor_id);
        LoadCustomerDetailsTab();
    }
    else if (currentTabIndex == 3) {
        LoadPhoneNumeberTab();
    }
    else if (currentTabIndex == 4) {
        LoadBillingInformationTab();
        isBillingPageVisible = true;
    }
    else if (currentTabIndex == 5) {
        LoadSignupCompleTab();
        GetSignUpCompleteDisplayInfo();
        isBillingPriceZero = false;
        price = (Number(selectedProductPrize) + Number(extraPersonalNumber) - Number(selectedProdcutDiscount)).toFixed(2);
        if (price == "0.00") {
            isBillingPriceZero = true;
        }
        if (isBillingPriceZero == true) {
            $("#trCreditCard").hide();
            $("#trCreditCardEnding").hide();
            $(".trACH").hide();
        }
        else {
            var isCreditCardSelected = $("#rblCreditCard").attr("checked");
            if (isCreditCardSelected) {
                $("#trCreditCard").show();
                $("#trCreditCardEnding").show();
            }
        }
        if (isBillingSectionHidden) {
            $("#trCreditCard").hide();
            $("#trCreditCardEnding").hide();
            $(".trACH").hide();
        }
    }
    else if (currentTabIndex == 6) {

    }
}
/*----------------------  Tab load----------------------------*/
function LoadRequestDemoTab() {
    SetNextButton(true);
}

function LoadSponosrSelectTab() {

    SetNextButton(true);
}

function LoadCustomerDetailsTab() {
    SetNextButton(true);
    var isSponsorChange = false;
    var tempSponsorName = $("#ddlSponsor option:selected").text();
    if (sponsorName != tempSponsorName) {
        isSponsorChange = true;
    }
    sponsorName = $("#ddlSponsor option:selected").text();

    $("#lblSponsorName").html(sponsorName);


    if (productId == 0 || isSponsorChange) {
        LoadSponsorProduct();
    }

}
function LoadPhoneNumeberTab() {
    SetNextButton(true);

    var iProductId = $("#ddlProduct option:selected").val();
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/PhoneNumberPricingInformation",
        data: '{iProductId: "' + iProductId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            hideLoadingPanel();
            if (response.d.IsSuccess == true) {
                $("#divPhoneNumberPricingInformation").html(response.d.strPhonePricingInformation);
            }
            else { alert("Error in Gettign Phone Number Pricing Information"); }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
    var isImmediatelyNumber = $("#rblImmediatelyNumber").attr("checked");
    if (isImmediatelyNumber) {
        $("#txtPhoneNumber").attr("disabled", "disabled");
    }
    else {
        $("#txtPhoneNumber").removeAttr("disabled");
    }
}

function LoadBillingInformationTab() {
    SetNextButton(true);

    var bLocal = false
    var bToll = false
    $("#divBackorder").hide();
    if (isLocalNumberOnOrder || isTollFreeNumberOnOrder) {
        if (isLocalNumberOnOrder && isTollFreeNumberOnOrder) {
            $("#divBackorder").html('You have requested a Local/Toll Free number that is not currently available. It will take 3 to 5 days to get one for you. In the meantime, you may use all on-net features, including messaging and calling. Text messaging and inbound calling will not be available until you are assigned a number. We will notify you when your number arrives.');
        }
        else if (isLocalNumberOnOrder) {
            $("#divBackorder").html('You have requested a Local number that is not currently available. It will take 3 to 5 days to get one for you. In the meantime, you may use all on-net features, including messaging and calling. Text messaging and inbound calling will not be available until you are assigned a number. We will notify you when your number arrives.');

        }
        else if (isTollFreeNumberOnOrder) {
            $("#divBackorder").html('You have requested a Toll Free number that is not currently available. It will take 3 to 5 days to get one for you. In the meantime, you may use all on-net features, including messaging and calling. Text messaging and inbound calling will not be available until you are assigned a number. We will notify you when your number arrives.');

        }
        $("#divBackorder").show();

    }
    //var txtLocalPhoneNumberQuantity = $("#txtLocalPhoneNumberQuantity");
    //if (txtLocalPhoneNumberQuantity.val().trim() != '' && txtLocalPhoneNumberQuantity.val() > 0) {
    //    bLocal = true;
    //}

    //var txtTollFreeeQauntity = $("#txtTollFreeeQauntity");
    //if (txtTollFreeeQauntity.val().trim() != '' && txtTollFreeeQauntity.val() > 0) {
    //    bToll = true;
    //}
    if (limitedNumberTypeId == null || (!isTollFreeNumberRequested && !isLocalNumberRequested)) {
        $(".divTollFreeNumbers").hide();
        $(".divLocalNumbers").hide();
        $(".divOrderedNumbers").hide();
    }
    else {
        $(".divOrderedNumbers").show();
        if (isTollFreeNumberRequested) {
            $(".divTollFreeNumbers").show();
        } else { $(".divTollFreeNumbers").hide(); }
        if (isLocalNumberRequested) {
            $(".divLocalNumbers").show();
        } else { $(".divLocalNumbers").hide(); }
    }
    

    var selectedProductName = $("#ddlProduct option:selected").text();
    $("#lblPurchaceProduct").html(selectedProductName);
    $("#lblSelectedProductName").html(selectedProductName);
    $("#signupProduct").html(selectedProductName);

    $("#lblBillingProductPrice").html(selectedProductPrize);

    $("#lblDiscount").html(selectedProdcutDiscount);



    totalPrice = (Number(selectedProductPrize) + Number(extraPersonalNumber) - Number(selectedProdcutDiscount)).toFixed(2);
    //var formattedPrice = GetFormattedStringFromDecimal(totalPrice.toFixed(2));
    $("#lblExtraPersonalNumber").html(extraPersonalNumber);
    $("#lblTotalPrice").html(totalPrice);
    $("#lblPrice").html(totalPrice);
}

function GetDecimalNumberFromFormattedString(number) {
    var Objnumbers = number.match(/[0-9]+/g);
    var beforDecimal = Objnumbers[0];
    var afterDecimal = Objnumbers[1];
    var decimalNumber = '';
    decimalNumber = decimalNumber.concat(beforDecimal, '.', afterDecimal);
    return decimalNumber;
}

function GetFormattedStringFromDecimal(number) {
    if (number % 1 == 0) {
        return number;
    }
    else {
        var decimalNumber = number.toString();
        var arrNumbers = decimalNumber.split('.');
        if (arrNumbers[1].length == 1) {
            arrNumbers[1] = arrNumbers[1].concat('0');
        }
        var formattedNumber = ''
        return formattedNumber.concat(arrNumbers[0], " and ", arrNumbers[1], " cents");
    }
}
function LoadSignupCompleTab() {
    SetNextButton(false);

    var isCreditCardSelected = $("#rblCreditCard").attr("checked");
    if (isCreditCardSelected) {
        $(".trCreditCard").show();
        $(".trACH").hide();
        DisplaySelectedCreditCardInfo();
    }
    else {
        $(".trCreditCard").hide();
        $(".trACH").show();
        DisplaySelectedACHInfo();
    }
}

function LoadAppDownloadTab() {
    BtnNextClick(true);
    $("#btnNext").hide();
    $("#btnBack").hide();

}
/*--------------------------------------------------------------------------*/

function DisplaySelectedCreditCardInfo() {
    //$("#lblCredtiCardName").html($("#ddlCardType option:selected").text());    
    $("#trCreditCard").show();
    $("#trCreditCardEnding").show();
    if ($("input[name='typeCard']:checked").val() == "AMX") {
        $("#lblCredtiCardName").html("American Express").text();
    }
    else if ($("input[name='typeCard']:checked").val() == "DIS") {
        $("#lblCredtiCardName").html("Discover").text();
    }
    else if ($("input[name='typeCard']:checked").val() == "MAS") {
        $("#lblCredtiCardName").html("Master Card").text();
    }
    else if ($("input[name='typeCard']:checked").val() == "VSA") {
        $("#lblCredtiCardName").html("Visa").text();
    }
    var credtiCardNumber = $("#txtCardNumber").val();
    credtiCardNumber = credtiCardNumber.replace(/\s/g, '');
    credtiCardNumber = credtiCardNumber.substr(credtiCardNumber.length - 4);

    $("#lblCardNumberEnding").html(credtiCardNumber);
}

function DisplaySelectedACHInfo() {
    //$("#lblAchType").html($("#ddlAccountType option:selected").text());
  //  $("#lblAchType").html($("input[name='accountType']:checked").text());

    if ($("input[name='accountType']:checked").val() == "1") {
        $("#lblAchType").html("Saving").text();
        }
    else { $("#lblAchType").html("Checking").text(); }

    var routing = $("#txtRoutingTransmitNumber").val();
    routing = routing.substr(routing.length - 4);

    var accountNumber = $("#txtAccountNumber").val();
    accountNumber = accountNumber.substr(accountNumber.length - 4);
    $("#trCreditCard").hide();
    $("#trCreditCardEnding").hide();
    $("#lblRoutingEnding").html(routing);
    $("#lblAccountNumberEnding").html(accountNumber);
}

function SetNextButton(bFlag) {
    if (bFlag) {
        $("#btnNext").text("Next");
    }
    else {
        $("#btnNext").text("Finish");
    }
}

function ShowCreditCardDetails(bFlag) {
    if (bFlag) {
        $("#creditCardDetails").show();
        $("#achDetails").hide();
    }
    else {
        $("#creditCardDetails").hide();
        $("#achDetails").show();
    }
}

function ShowUnitedStateAddress(bFlag) {
    if (bFlag) {
        $("#sectionUSAAddress").show();
        $("#sectionNonUSAAddress").hide();
    }
    else {
        $("#sectionUSAAddress").hide();
        $("#sectionNonUSAAddress").show();
    }
}
/*----------------------------- Reloading Master Images -----------------------------------*/
function ReloadMasterImages(sponsor_id) {
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/ReloadMasterImages",
        data: '{strSponsorId: "' + sponsor_id + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            //ShowLoadingPanel();
        },
        success: function (response) {
            //hideLoadingPanel();
            if (response.d.IsSuccess == true) {
                $("[id$='ImageCenter']").attr("src", response.d.CenterImageUrl);
                $("[id$='ImageLeft']").attr("src", response.d.LeftImageUrl);
                $("[id$='ImageRight']").attr("src", response.d.RightImageUrl);
                $("[id$='imgCellTopImage']").attr("src", response.d.CellTopImageUrl);
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

function IsInvalidBillingPeriod() {
    if ($("#lblBillingPeriod").text() == "-") {
        return true;
    }
    else {
        return false;
    }
}


/*-------------------------------------------------End LoadTabs-------------------------------------------------------------------------------*/

/**************************************** Save customer in VBOS *******************************************************************************/
//function SaveCustomerToVBOS() {
//    $.ajax({
//        type: "POST",
//        url: "SimpleSignUp.aspx/SaveCustomerToVBOS",
//        data: '',
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        beforeSend: function () {
//            ShowLoadingPanel();
//        },
//        success: function (response) {
//            hideLoadingPanel();
//            if (response.d == "true") {
//                BtnNextClick(true);
//            }
//            else {
//                alert("Error in saving customer in VBOS");
//            }
//        },
//        failure: function (response) {
//            hideLoadingPanel();
//            alert(response.d);
//        }
//    });
//}

/**************************************** END of Save customer in VBOS *******************************************************************************/

/*********************************************** Get Phone Number ******************************************************************************/
function GetPhoneNumber(countLocalPhoneNumber, countTollFreeNumber, requestLocalNumber, requestTollFreeNumber, isByPassGetPhoneNumber){
    var strLocalPhoneNumber = "Local";
    var strTollFreeNumber = "Toll Free";
    
    isLocalNumberOnOrder = false;
    isTollFreeNumberOnOrder = false;

    isTollFreeNumberRequested = $("#chkTollFreeNumber").is(':checked');
    isLocalNumberRequested = $("#chkLocalNumber").is(':checked');
    //var txtLocalPhoneNumberQuantity = $("#txtLocalPhoneNumberQuantity");
    //if (txtLocalPhoneNumberQuantity.val().trim() != '' && txtLocalPhoneNumberQuantity.val() > 0) {
    //    strLocalPhoneNumber = "Local";
    //    countLocalPhoneNumber = txtLocalPhoneNumberQuantity.val().trim();
    //    isByPassGetPhoneNumber = false;
    //}
    //else {
    //    countLocalPhoneNumber = 0;
    //}

    //var txtTollFreeeQauntity = $("#txtTollFreeeQauntity");
    //if (txtTollFreeeQauntity.val().trim() != '' && txtTollFreeeQauntity.val() > 0) {
    //    strTollFreeNumber = "Toll Free";
    //    countTollFreeNumber = txtTollFreeeQauntity.val().trim();
    //    isByPassGetPhoneNumber = false;
    //}
    //else {
    //    countTollFreeNumber = 0;
    //}
    
    if (isByPassGetPhoneNumber) {
        extraPersonalNumber = 0;
        $("#lblTollFreeNumber").html('');
        $("#lblLocalNumber").html('');
        $("#lblNumberResponseMessage").html('');
        $("#lblOrderedTollFreeNumbers").html('');
        $("#lblOrderedLocalNumbers").html('');
        BtnNextClick(true);
        return;
    }

    
    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/GetPhoneNumber",
        data: '{strLocalPhoneNumber: "' + strLocalPhoneNumber + '", strTollFreeNumber: "' + strTollFreeNumber + '",strPhoneNumber:"' + strPhoneNumber + '",isFromAnyWhere:"' + isFromAnyWhere + '",countLocalPhoneNumber:"' + countLocalPhoneNumber + '",countTollFreeNumber:"' + countTollFreeNumber + '",strProductId:"' + productId + '",lstNumbers:' + JSON.stringify(lstPhoneNumber) + ',requestTollFreeNumber:' + requestTollFreeNumber + ',requestLocalNumber:' + requestLocalNumber + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            hideLoadingPanel();
            $("#lblLocalNumber").html("");
            $("#lblTollFreeNumber").html("");
            $("#lblNumberResponseMessage").html('');
            $("#lblOrderedTollFreeNumbers").html('');
            $("#lblOrderedLocalNumbers").html('');
            if (response.d.IsSuccess == true) {
                if (response.d.IsInvalidQuantity == true) {
                    $("#lblNumberResponseMessage").show().html(response.d.ResponseMessage);
                    $("#lblExtraNumberCostMessage").hide();
                } else {
                    if (response.d.lstPhoneNumbers.count == "" || response.d.lstPhoneNumbers.count == null) {
                        lstPhoneNumber = response.d.lstPhoneNumbers;
                    }
                    if (response.d.TollFreeNumber == "") {
                        if (countTollFreeNumber > 0) {
                            $("#lblTollFreeNumber").html("On Order");
                            $("#lblOrderedTollFreeNumbers").html("On Order");
                            isTollFreeNumberOnOrder = true;
                            isTollFreeNumberReturned = true;
                        }
                    }
                    else {
                        $("#lblTollFreeNumber").html(response.d.TollFreeNumber);
                        $("#lblOrderedTollFreeNumbers").html(response.d.TollFreeNumber);
                        isTollFreeNumberReturned = true;
                    }

                    if (response.d.LocalNumber == "") {
                        if (countLocalPhoneNumber > 0) {
                            $("#lblLocalNumber").html("On Order");
                            $("#lblOrderedLocalNumbers").html("On Order");
                            isLocalNumberOnOrder = true;
                            isLocalNumberReturned = true;
                            if (isFromAnyWhere == 1) {
                                isFromAnyWhereReturned = true;
                            } else{
                                isFromGeographicReturned = true;
                            }
                        }
                    } else {
                        $("#lblLocalNumber").html(response.d.LocalNumber);
                        $("#lblOrderedLocalNumbers").html(response.d.LocalNumber);
                        isLocalNumberReturned = true;
                        if (isFromAnyWhere == 1) {
                            isFromAnyWhereReturned = true;
                        } else {
                            isFromGeographicReturned = true;
                        }
                    }
                    extraPersonalNumber = response.d.iExtraPersonalNumber;
                    ExtraPersonalNumberCount = response.d.ExtraPersonalNumberCount;
                    BtnNextClick(true);
                }
            } else {
                if (countLocalPhoneNumber > 0) {
                    $("#lblLocalNumber").html("On Order");
                    $("#lblOrderedLocalNumbers").html("On Order");
                    isLocalNumberOnOrder = true;
                    isLocalNumberReturned = true;
                }
                if (countTollFreeNumber > 0) {
                    $("#lblTollFreeNumber").html("On Order");
                    $("#lblOrderedTollFreeNumbers").html("On Order");
                    isTollFreeNumberOnOrder = true;
                    isTollFreeNumberReturned = true;
                }
                BtnNextClick(true);
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

/***********************************************************************************************************************************************/

function ReleaseNumberAndGetPhoneNumber() {
    
    var countLocalPhoneNumber = 0;
    var countTollFreeNumber = 0;
    var requestLocalNumber = false;
    var requestTollFreeNumber = false;
    var ReleaseLocal = false;
    var ReleaseTollFree = false;
    var isByPassGetPhoneNumber = true;
    isTollFreeNumberRequested = $("#chkTollFreeNumber").is(':checked');
    isLocalNumberRequested = $("#chkLocalNumber").is(':checked');
    

    if (isTollFreeNumberRequested || isLocalNumberRequested) {
        isByPassGetPhoneNumber = false;
    }
    if (isTollFreeNumberRequested) {
        countTollFreeNumber = 1;
        if (isTollFreeNumberReturned) {
            requestTollFreeNumber = false;
        } else {
            requestTollFreeNumber = true;
        }
    } else {
        if (isTollFreeNumberReturned) {
            ReleaseTollFree = true;
        }
    }

    if ($("#rblImmediatelyNumber").is(':checked')) {
        isFromAnyWhere = 1;
    }
    else {
        isFromAnyWhere = 0;
    }

    if (isLocalNumberRequested) {
        countLocalPhoneNumber = 1;
        if (isLocalNumberReturned) {
            if (isFromAnyWhereReturned && isFromAnyWhere == 1) {
                if (isFromAnyWhereReturned) {
                    requestLocalNumber = false;
                    
                } 
            } else if (isFromAnyWhere == 1) {
                requestLocalNumber = true;
                ReleaseLocal = true;
            }

            if (isFromGeographicReturned && isFromAnyWhere == 0) {
                if (isFromGeographicReturned) {
                    var strPlainNumber = parseInt($("#txtPhoneNumber").val().replace(/[^0-9\.]/g, ''), 10);
                    if (strPhoneNumber == strPlainNumber) {
                        requestLocalNumber = false;
                    } else {
                        requestLocalNumber = true;
                        ReleaseLocal = true;
                    }
                }
            } else if (isFromAnyWhere == 0) {
                requestLocalNumber = true;
                ReleaseLocal = true;
            }
                
        } else {
            requestLocalNumber = true;
        }
    } else {
        if (isLocalNumberReturned) {
            ReleaseLocal = true;
        }
    }

    strPhoneNumber = parseInt($("#txtPhoneNumber").val().replace(/[^0-9\.]/g, ''), 10);

    if (ReleaseLocal || ReleaseTollFree) {

        var lstNumbers = JSON.stringify(lstPhoneNumber);
        $.ajax({
            type: "POST",
            url: "SimpleSignUp.aspx/ReleasephoneNumber",
            data: '{lstPhoneNumber:' + lstNumbers + ',ReleaseLocal:"' + ReleaseLocal + '",ReleaseTollFree:"' + ReleaseTollFree + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                ShowLoadingPanel();
            },
            success: function (response) {
                hideLoadingPanel();

                lstPhoneNumber = response.d.lstNumbers;
                if (lstNumbers.count <= 0) { lstPhoneNumber = null; }

                if (ReleaseLocal) {
                    isLocalNumberReturned = false;
                    if (isFromGeographicReturned) {
                        isFromGeographicReturned = false;
                    } else if (isFromAnyWhereReturned) {
                        isFromAnyWhereReturned = false;
                    }
                }
                if (ReleaseTollFree) {
                    isTollFreeNumberReturned = false;
                }
                GetPhoneNumber(countLocalPhoneNumber, countTollFreeNumber, requestLocalNumber, requestTollFreeNumber, isByPassGetPhoneNumber);

            },
            failure: function (response) {
                hideLoadingPanel();
            }
        });
    } else {

        GetPhoneNumber(countLocalPhoneNumber, countTollFreeNumber, requestLocalNumber, requestTollFreeNumber, isByPassGetPhoneNumber);
    }
}
