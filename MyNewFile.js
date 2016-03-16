var arrayNew=new Array();
var arrayNew1=new Array();
$(document).ready(function () {
    var isMobile = {
        Android: function () {
            return navigator.userAgent.match(/Android/i);
        },
        BlackBerry: function () {
            return navigator.userAgent.match(/BlackBerry/i);
        },
        iOS: function () {
            return navigator.userAgent.match(/iPhone|iPad|iPod/i);
        },
        Opera: function () {
            return navigator.userAgent.match(/Opera Mini/i);
        },
        Windows: function () {
            return navigator.userAgent.match(/IEMobile/i);
        },
        any: function () {
            return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
        }
    };
    if (isMobile.any()) {
        if ($("#divUSRaditionButon1").hasClass("col-xs-5")) { $("#divUSRaditionButon1").removeClass("col-xs-5"); }
        if ($("#divUSRaditionButon2").hasClass("col-xs-7")) { $("#divUSRaditionButon2").removeClass("col-xs-7"); }
        $("#divUSRaditionButon1").addClass("col-xs-12");
        $("#divUSRaditionButon2").addClass("col-xs-12");
        $("#SignUpPageWraperDiv").removeClass("SignUpPageWraper");
        $("#SignUpPageWraperDiv").addClass("SignUpPageWraperCell");
    } else {
        if ($("#divUSRaditionButon1").hasClass("col-xs-12")) { $("#divUSRaditionButon1").removeClass("col-xs-12"); }
        if ($("#divUSRaditionButon2").hasClass("col-xs-12")) { $("#divUSRaditionButon2").removeClass("col-xs-12"); }
        $("#divUSRaditionButon1").addClass("col-xs-5");
        $("#divUSRaditionButon2").addClass("col-xs-7");
        $("#SignUpPageWraperDiv").removeClass("SignUpPageWraperCell");
        $("#SignUpPageWraperDiv").addClass("SignUpPageWraper");
    }

    $("#txtFirstName").blur(function () {
        if (!ValidateFirstNameLastName($(this).val())) {
            $(this).attr("required", true);
            $(this).addClass("invalidFirstName");
            $(this).attr("title", "Invalid First Name.");
            $("#lblInvalidFirstName").remove();
            var isInValid = $("#txtFirstName").hasClass("invalidFirstName");
            if (isInValid) {
                $("<label id='lblInvalidFirstName' for='txtFirstName' class='errorLable' generated='true' >Invalid First Name.</label>").insertBefore(".invalidFirstName");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeClass("invalidFirstName");
            $(this).removeAttr("title");
            $("#lblInvalidFirstName").remove();
        }
    });

    $("#txtLastName").blur(function () {
        if (!ValidateFirstNameLastName($(this).val())) {
            $(this).attr("required", true);
            $(this).addClass("invalidLastName");
            $(this).attr("title", "Invalid Last Name.");
            $("#lblInvalidLastName").remove();
            var isInValid = $("#txtLastName").hasClass("invalidLastName");
            if (isInValid) {
                $("<label id='lblInvalidLastName' for='txtLastName' class='errorLable' generated='true' >Invalid Last Name.</label>").insertBefore(".invalidLastName");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeClass("invalidLastName");
            $(this).removeAttr("title");
            $("#lblInvalidLastName").remove();
        }
    });

    $("#txtSolutionNumber").blur(function () {
        if ($("#txtSolutionNumber").val().trim() != "") {
            IsExistsSolutionNumberInbrand();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeAttr("title");
            $(this).removeClass("invalidSolutionNumber");
            $("#lblSolutionNumber").remove();
        }

    });

    $("#txtPreferredUserName").blur(function () {
        ValidateUserName();
    });

    $("#txtPassword").blur(function () {
        ValidatePassword();
    });

    $("#txtRepeatPassword").blur(function () {
        if (ValidatePassword()) {
            ValidatePasswordMatch();
        }
    });

    $("#txtReenterAccountNumber").blur(function () {
        ValidateAccountNumberMatch();
    });

    $("#txtEmailAddress").blur(function () {
        if (!ValidateEmail($(this).val())) {
            $(this).attr('required', true);
            $(this).addClass('invalidEmail');
            $(this).attr("title", "Invalid Email.");
            $("#lblInvalidEmail").remove();
            var isInValid = $("#txtEmailAddress").hasClass("invalidEmail");
            if (isInValid) {
                $("<label id='lblInvalidEmail' for='txtEmailAddress' class='errorLable' generated='true' >Invalid Email.</label>").insertBefore(".invalidEmail");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr('required');
            $(this).removeClass('invalidEmail');
            $(this).removeAttr('title');
            $("#lblInvalidEmail").remove();
        }
    });

    $("#txtVoiceMailPIN").blur(function () {
        var voicemailpin = $("#txtVoiceMailPIN").val().trim();
        if (voicemailpin != '' && voicemailpin.length < 4) {
            $("#txtVoiceMailPIN").attr('required', true);
            $("#txtVoiceMailPIN").attr('title', 'Voice Mail PIN should be of minimum 4 (numeric digits).');
            $("#txtVoiceMailPIN").addClass('invalidVoiceMailPin');
            $("#lblInvalidVoiceMailPin").remove();
            var isInValid = $("#txtVoiceMailPIN").hasClass("invalidVoiceMailPin");
            if (isInValid) {
                $("<label id='lblInvalidVoiceMailPin' for='txtVoiceMailPIN' class='errorLable' generated='true' >Voice Mail PIN should be of minimum 4 (numeric digits).</label>").insertBefore(".invalidVoiceMailPin");
            }
            setBorderColor();
            return false;
        }
        $("#txtVoiceMailPIN").removeAttr('required');
        $("#txtVoiceMailPIN").removeAttr('title');
        $("#txtVoiceMailPIN").removeClass('invalidVoiceMailPin');
        $("#lblInvalidVoiceMailPin").remove();
        return true;
    });

    $("#txtMobileNumber").blur(function () {
        var mobileNumber = $(this).val().trim();
        if (mobileNumber != '') {
            var thisMobileNumber = $("#txtMobileNumber");
            $.ajax({
                type: "POST",
                url: "SimpleSignUp.aspx/IsValidPhoneNumber",
                data: '{strPhoneNumber: "' + mobileNumber + '"}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: function () {
                    ShowLoadingPanel();
                },
                success: function (response) {
                    if (response.d.IsSuccess) {
                        thisMobileNumber.removeAttr('required');
                        $("#lblInvalidMobileNumber").remove();
                        thisMobileNumber.removeClass('invalidMobileNumber');
                        thisMobileNumber.val(response.d.E164TypePhoneNumber);
                    }
                    else {
                        thisMobileNumber.attr('required', true);
                        thisMobileNumber.addClass('invalidMobileNumber');
                        $("#lblInvalidMobileNumber").remove();
                        var isInValid = $("#txtMobileNumber").hasClass("invalidMobileNumber");
                        if (isInValid) {
                            $("<label id='lblInvalidMobileNumber' for='txtMobileNumber' class='errorLable' generated='true' >Invalid Mobile Number.</label>").insertBefore(".invalidMobileNumber");
                        }
                        setBorderColor();
                    }
                    hideLoadingPanel();
                },
                failure: function (response) {
                    hideLoadingPanel();
                    alert(response.d);
                }
            });
        }
        else {
            $(this).removeAttr('required');
            $(this).removeAttr('title');
            $(this).removeClass('invalidMobileNumber');
            $("#lblInvalidMobileNumber").remove();
        }
    });

    $("#txtAnswer").blur(function () {
        $(this).removeAttr('required');
    });

    //$('.numeric').bind("cut copy paste", function (e) {
    //    e.preventDefault();
    //});

    //$('input[type=password]').bind("cut copy paste", function (e) {
    //    e.preventDefault();
    //});

    $(".numeric").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });

    $(".alphabates").keydown(function (e) {
        var code = e.keyCode || e.which;
        if ((code < 65 || code > 90) && (code < 97 || code > 122) && code != 32 && code != 46 && code != 8 && code != 9) {
            return false;
        }
    });

   

    // Validation for billing page in credit card and ACH with united state and non united state
    $("#txtCardNumber").blur(function () {
        $(this).removeAttr('required');
    });

    //$("#ddlCardType").blur(function () {
    //    $(this).removeAttr('required');
    //});

    $("#txtNameOnCard").blur(function () {
        var nameOnCard = $(this).val().trim();
        if (ValidateFirstNameLastName(nameOnCard)) {
            $(this).removeAttr('required');
            $(this).removeClass("invalidNameOnCard");
            $("#lblInvalidNameOnCard").remove();
        }
        else {
            $(this).attr('required', true);
            $(this).addClass('invalidNameOnCard');
            $("#lblInvalidNameOnCard").remove();
            var isInValid = $("#txtNameOnCard").hasClass("invalidNameOnCard");
            if (isInValid) {
                $("<label id='lblInvalidNameOnCard' for='txtNameOnCard' class='errorLable' generated='true' >Invalid Name.</label>").insertBefore(".invalidNameOnCard");
            }
            setBorderColor();
        }
    });

    $("#ddlMonth").click(function () {
        if ($("#ddlMonth Option:selected").val() != "0") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });

    $("#ddlYear").click(function () {
        if ($("#ddlYear Option:selected").val() != "- select -") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });

    $("#ddlMonth").blur(function () {
        if ($("#ddlMonth Option:selected").val() != "0") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });
    var n;

    $("#ddlYear").blur(function () {
        if ($("#ddlYear Option:selected").val() != "- select -") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });


    $("#txtCardSecurityNumber").blur(function () {
        $(this).removeAttr('required');
    });

    $("#txtAddressLine1").blur(function () {
        $(this).removeAttr('required');
    });

    //$("#txtAddressLine2").blur(function () {
    //    $(this).removeAttr('required');
    //});
    var t
    $("#txtCity").blur(function () {
        if (!ValidateFirstNameLastName($(this).val())) {
            $(this).attr("required", true);
            $(this).addClass("invalidCity");
            $(this).attr("title", "Invalid City ");
            $("#lblInvalidCity").remove();
            var isInValid = $("#txtCity").hasClass("invalidCity");
            if (isInValid) {
                $("<label id='lblInvalidCity' for='txtCity' class='errorLable' generated='true' >Invalid City </label>").insertBefore(".invalidCity");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeClass("invalidCity");
            $(this).removeAttr("title");
            $("#lblInvalidCity").remove();
        }
    });

   
    $("#ddlState").click(function () {
        if ($("#ddlState Option:selected").val() != "0") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });

    $("#ddlState").blur(function () {
        if ($("#ddlState Option:selected").val() != "0") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });

    $("#txtZipCode").blur(function () {
        var zipCode = $(this).val().trim();
        if (zipCode == '' || zipCode.length == 5) {
            $(this).removeAttr('required');
            $(this).removeClass('invalidZipCode');
            $("#lblInvalidZipCode").remove();
        }
        else {
            $(this).attr('required', true);
            $(this).addClass('invalidZipCode');
            $("#lblInvalidZipCode").remove();
            var isInValid = $("#txtZipCode").hasClass("invalidZipCode");
            if (isInValid) {
                $("<label id='lblInvalidZipCode' for='txtZipCode' class='errorLable' generated='true' >Invalid Zipcode.</label>").insertBefore(".invalidZipCode");
            }
            setBorderColor();
        }
    });

    $("#txtInternationalStreet1").blur(function () {
        $(this).removeAttr('required');
    });

    //$("#txtInternationalStreet2").blur(function () {
    //    $(this).removeAttr('required');
    //});

    $("#txtInternationalCity").blur(function () {
        if (!ValidateFirstNameLastName($(this).val())) {
            $(this).attr("required", true);
            $(this).addClass("invalidInternationalCity");
            $(this).attr("title", "Invalid International City ");
            $("#lblInvalidInternationalCity").remove();
            var isInValid = $("#txtInternationalCity").hasClass("invalidInternationalCity");
            if (isInValid) {
                $("<label id='lblInvalidInternationalCity' for='txtInternationalCity' class='errorLable' generated='true' >Invalid City </label>").insertBefore(".invalidInternationalCity");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeClass("invalidInternationalCity");
            $(this).removeAttr("title");
            $("#lblInvalidInternationalCity").remove();
        }
    });

    $("#txtInternationalProvince").blur(function () {
        if (!ValidateFirstNameLastName($(this).val())) {
            $(this).attr("required", true);
            $(this).addClass("invalidInternationalProvince");
            $(this).attr("title", "Invalid CountryProvince ");
            $("#lblInvalidInternationalProvince").remove();
            var isInValid = $("#txtInternationalProvince").hasClass("invalidInternationalProvince");
            if (isInValid) {
                $("<label id='lblInvalidInternationalProvince' for='txtInternationalProvince' class='errorLable' generated='true' >Invalid Country/Province </label>").insertBefore(".invalidInternationalProvince");
            }
            setBorderColor();
        }
        else {
            $(this).removeAttr("required");
            $(this).removeClass("invalidInternationalProvince");
            $(this).removeAttr("title");
            $("#lblInvalidInternationalProvince").remove();
        }
    });

    //$("#txtInternationalPost").blur(function () {
    //    var zipCode = $(this).val().trim();
    //    if (zipCode == '' || zipCode.length == 10) {
    //        $(this).removeAttr('required');
    //        $(this).removeClass('invalidZipCode');
    //        $("#lblInvalidZipCode").remove();
    //    }
    //    else {
    //        $(this).attr('required', true);
    //        $(this).addClass('invalidInternationalPost');
    //        $("#lblInvalidInternationalPost").remove();
    //        var isInValid = $("#txtInternationalPost").hasClass("invalidInternationalPost");
    //        if (isInValid) {
    //            $("<label id='lblInvalidInternationalPost' for='txtInternationalPost' class='errorLable' generated='true' >Invalid Zipcode.</label>").insertBefore(".invalidInternationalPost");
    //        }
    //        setBorderColor();
    //    }
    //});

    $("#txtInternationalPost").blur(function () {
        $(this).removeAttr('required');
    });

       $('#txtInternationalPost').keydown(function (e) {
           if (e.shiftKey || e.ctrlKey || e.altKey ) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 9) || (key == 35) || (key == 37) || (key == 38) || (key == 39) || (key == 46) ||
                (key >= 35 && key <= 40) || (key >= 48 && key <= 57) || (key >= 65 && key <= 90) ||
                (key >= 96 && key <= 105))) {
                e.preventDefault();
            }
        }
    });
   

    $("#ddlInternationalCountry").click(function () {
        if ($("#ddlInternationalCountry Option:selected").val() != "0") {
            $(this).removeAttr('required');
        }
        else { $(this).attr("required", true); }
    });

   

    $("#txtNameOnAccount").blur(function () {
        var nameOnCard = $(this).val().trim();
        if (ValidateFirstNameLastName(nameOnCard)) {
            $(this).removeAttr('required');
            $(this).removeClass("invalidNameOnAccount");
            $("#lblInvalidNameOnAccount").remove();
        }
        else {
            $(this).attr('required', true);
            $(this).addClass('invalidNameOnAccount');
            $("#lblInvalidNameOnAccount").remove();
            var isInValid = $("#txtNameOnAccount").hasClass("invalidNameOnAccount");
            if (isInValid) {
                $("<label id='lblInvalidNameOnAccount' for='txtNameOnAccount' class='errorLable' generated='true' >Invalid Name.</label>").insertBefore(".invalidNameOnAccount");
            }
            setBorderColor();
        }
    });


    $("#txtBankName").blur(function () {
        $(this).removeAttr('required');
    });

    $("#txtRoutingTransmitNumber").blur(function () {
        if ($("#txtRoutingTransmitNumber").val().length == 9) {
            $("#txtRoutingTransmitNumber").removeAttr('required');
            $("#lblInvalidRoutingCodeError").remove();
            $("#txtRoutingTransmitNumber").removeClass('invalidRoutingCode');


        }
        else {
            $("#txtRoutingTransmitNumber").attr('required', true);
            $("#txtRoutingTransmitNumber").addClass('invalidRoutingCode');
            $("#lblInvalidRoutingCodeError").remove();
            var isInValid = $("#txtRoutingTransmitNumber").hasClass("invalidRoutingCode");
            if (isInValid) {
                $("<label id='lblInvalidRoutingCodeError' for='txtRoutingTransmitNumber' class='errorLable' generated='true' >Invalid Routing Number.</label>").insertBefore(".invalidRoutingCode");
            }
            setBorderColor();
        }
    });

    $('#txtRoutingTransmitNumber').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 9) || (key == 46) || (key >= 35 && key <= 40) || (key >= 48 && key <= 57) || (key >= 96 && key <= 105))) {
                e.preventDefault();
            }
        }
    });


    $("#txtAccountNumber").blur(function () {
        $(this).removeAttr('required'); 
    });

    $('#txtAccountNumber').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 9) || (key == 46) || (key >= 35 && key <= 40) || (key >= 48 && key <= 57) || (key >= 96 && key <= 105))) {
                e.preventDefault();
            }
        }
    });


    $("#txtReenterAccountNumber").blur(function () {
        $(this).removeAttr('required');
    });

    $("#txtAuthCode").blur(function () {
        if ($("#txtAuthCode").val() == "") {
            $("#lblAuthCodeError").remove();
        }
    });
});

function IsExistsSolutionNumberInbrand() {
    var txtSolutionNumber = $("#txtSolutionNumber");
    if (txtSolutionNumber.val().trim() != "") {
        txtSolutionNumber.removeAttr("required");
        txtSolutionNumber.removeAttr("title");
        txtSolutionNumber.removeClass("invalidSolutionNumber");
        $("#lblSolutionNumber").remove();
        $.ajax({
            type: "POST",
            url: "SimpleSignUp.aspx/isExistsSolutionNumberInbrand",
            data: '{strSolutionNumber: "' + txtSolutionNumber.val().trim() + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                ShowLoadingPanel();
            },
            success: function (response) {
                if (response.d.isExists == false) {
                    txtSolutionNumber.removeAttr("required");
                    txtSolutionNumber.removeClass("invalidSolutionNumber");
                    $("#lblSolutionNumber").remove();
                }
                else {
                    txtSolutionNumber.attr("required", true);
                    txtSolutionNumber.addClass("invalidSolutionNumber");
                    $("#lblSolutionNumber").remove();
                    var isInValid = $("#txtSolutionNumber").hasClass("invalidSolutionNumber");
                    if (isInValid) {
                        $("<label id='lblSolutionNumber' for='txtSolutionNumber' class='errorLable' generated='true' >Solution Number Taken</label>").insertBefore(".invalidSolutionNumber");
                    }
                    setBorderColor();
                }
                hideLoadingPanel();
            },
            failure: function (response) {
                hideLoadingPanel();
                alert(response.d);
            }
        });
    }
    else {
        txtSolutionNumber.attr("required", true);
        txtSolutionNumber.addClass("invalidSolutionNumber");
        $("#lblSolutionNumber").remove();
    }
}

function ValidateUserName() {
    var txtUserName = $("#txtPreferredUserName");
    if (ValidateUserNameExpression(txtUserName.val())) {
        txtUserName.removeAttr("required");
        txtUserName.removeAttr("title");
        txtUserName.removeClass("invalidUserName");
        $("#lblInvalidPreferredUserName").remove();
        $.ajax({
            type: "POST",
            url: "SimpleSignUp.aspx/ValidateUserName",
            data: '{userName: "' + txtUserName.val() + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                ShowLoadingPanel();
            },
            success: function (response) {
                if (response.d == "true") {
                    txtUserName.removeAttr("required");
                    txtUserName.removeClass("invalidUserName");
                    SetUserNameTaken();
                } else {
                    txtUserName.attr("required", true);
                    txtUserName.addClass("invalidUserName");
                    SetUserNameTaken();
                }

                hideLoadingPanel();
            },
            failure: function (response) {
                hideLoadingPanel();
                alert(response.d);
            }
        });
    }
    else {
        txtUserName.attr("required", true);
        txtUserName.addClass("invalidUserName");
        txtUserName.attr("title", "Preferred User Name must be at least 5 digits and may only use numbers, letters or periods.");
        $("#lblInvalidPreferredUserName").remove();
        $("#lblUserNameTaken").remove();
        var isInValid = $("#txtPreferredUserName").hasClass("invalidUserName");
        if (isInValid) {
            $("<label id='lblInvalidPreferredUserName' for='txtPreferredUserName' class='errorLable' generated='true' >Preferred User Name must be at least 5 digits and may only use numbers, letters or periods.</label>").insertBefore(".invalidUserName");
        }
        setBorderColor();

    }
}

function SetUserNameTaken() {
    $("#lblUserNameTaken").remove();
    $("#lblInvalidPreferredUserName").remove();
    var isInValid = $("#txtPreferredUserName").hasClass("invalidUserName");
    if (isInValid) {
        $("<label id='lblUserNameTaken' for='txtPreferredUserName' class='errorLable' generated='true' >User Name Taken</label>").insertBefore(".invalidUserName");
    }
    //var validator = $("#formSignUp").validate({
    //    rules: {
    //        txtPreferredUserName: {
    //            userNameTaken: "#txtPreferredUserName"
    //        }
    //    },
    //    messages: {
    //        txtPreferredUserName: "User Name Taken"
    //    }
    //});

    //if (validator.form()) {
    //    return true;
    //}
    //else {
    //    return false;
    //}
}

function ValidatePassword() {
    var password = $("#txtPassword").val().trim();
    if (password != '' && password.length < 6) {
        $("#txtPassword").attr('required', true);
        $("#txtPassword").attr('title', 'Password should be at least 6 characters.');
        $("#txtPassword").addClass("invalidPassword");
        $("#lblInvalidPassword").remove();
        var isInValid = $("#txtPassword").hasClass("invalidPassword");
        if (isInValid) {
            $("<label id='lblInvalidPassword' for='txtPassword' class='errorLable' generated='true' >Password should be at least 6 characters.</label>").insertBefore(".invalidPassword");
        }
        setBorderColor();
        return false;
    }
    $("#txtPassword").removeAttr('required');
    $("#txtPassword").removeAttr('title');
    $("#lblInvalidPassword").remove();
    $("#txtPassword").removeClass("invalidPassword");
    return true;
}

function ValidatePasswordMatch() {
    $("#lblPasswordNoMatch").remove();
    var password = $("#txtPassword").val().trim();
    var repeatPassword = $("#txtRepeatPassword").val().trim();
    if (password != repeatPassword) {
        $("#txtRepeatPassword").addClass("passwordMisMatch");
        $("<label id='lblPasswordNoMatch' for='txtRepeatPassword' class='errorLable' generated='true' >Enter Repeat Password Same as Password</label>").insertBefore(".passwordMisMatch");
        return false;
    }
    $("#txtRepeatPassword").removeAttr('required');
    $("#txtRepeatPassword").removeClass("passwordMisMatch");
    return true;
}

function ValidateAccountNumberMatch() {
    //var validator = $("#divBillingInformation").validate({
    //    rules: {
    //        txtAccountNumber: "required",
    //        txtReenterAccountNumber: {
    //            equalTo: "#txtAccountNumber"
    //        }
    //    },
    //    messages: {
    //        txtAccountNumber: "",
    //        txtReenterAccountNumber: "Enter Reenter Account Number Same as Account Number"
    //    }
    //});
    //if (validator.form()) {
    //    return true;
    //}
    //else {
    //    return false;
    //}
    $("#lblAccNumberNoMatch").remove();
    var accNumber = $("#txtAccountNumber").val().trim();
    var repeatAccNumber = $("#txtReenterAccountNumber").val().trim();
    if (repeatAccNumber != accNumber) {
        $("#txtReenterAccountNumber").addClass("accNumberMisMatch");
        $("<label id='lblAccNumberNoMatch' for='txtReenterAccountNumber' class='errorLable' generated='true' >Reenter account number not match</label>").insertBefore(".accNumberMisMatch");
        return false;
    }
    $("#txtReenterAccountNumber").removeClass("accNumberMisMatch");
    return true;
}

function ValidateSponsorSelect() {
    var selectedSponsor = $("#ddlSponsor option:selected").val();
    var selectedSponsorName = $("#ddlSponsor option:selected").text();
    if (selectedSponsorName == "Primerica") {
        if (selectedSponsor.trim() == '') {
            return false;
        }
        $("#divSolutionNumeber").show();
    }
    else {
        $("#divSolutionNumeber").hide();
    }
    return true;
}

function ValidateCustomerDetails() {
    ClearValidation();

    var selectedSponsorName = $("#ddlSponsor option:selected").text();
    var selectedProductValue = $("#ddlProduct option:selected").val();

    var txtFistname = $("#txtFirstName");
    var txtLastName = $("#txtLastName");
    var txtSolutionNumber = $("#txtSolutionNumber");
    var txtPreferredUserName = $("#txtPreferredUserName");
    var txtPassword = $("#txtPassword");
    var txtRepeatPassword = $("#txtRepeatPassword");
    var txtVoiceMailPIN = $("#txtVoiceMailPIN");
    var txtEmailAddress = $("#txtEmailAddress");
    var txtMobileNumber = $("#txtMobileNumber");
    var txtSecretQuestion = $("#txtSecretQuestion");
    var txtAnswer = $("#txtAnswer");

    var bFlag = true;

    if (txtPassword.val().trim() == '') {
        txtPassword.attr('required', true);
        bFlag = bFlag && false;
    } else if (ValidatePasswordMatch() == false) {
        txtRepeatPassword.attr('required', true);
        bFlag = bFlag && false;
    }

    if (selectedProductValue == undefined || selectedProductValue.trim() == '') {
        $("#ddlProduct").attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtFistname.val().trim() == '') {
        txtFistname.attr('required', true);
        bFlag = bFlag && false;
    } else if (txtFistname.hasClass("invalidFirstName")) {
        txtFistname.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtLastName.val().trim() == '') {
        txtLastName.attr('required', true);
        bFlag = bFlag && false;
    }
    else if (txtLastName.hasClass("invalidLastName")) {
        txtLastName.attr('required', true);
        bFlag = bFlag && false;
    }

    if (selectedSponsorName == "Primerica") {
        if (txtSolutionNumber.val().trim() == '') {
            txtSolutionNumber.attr('required', true);
            bFlag = bFlag && false;
        }
        else if (txtSolutionNumber.hasClass('invalidSolutionNumber')) {
            txtSolutionNumber.attr('required', true);
            bFlag = bFlag && false;

        }
    }

    if (txtPreferredUserName.val().trim() == '') {
        txtPreferredUserName.attr('required', true);
        bFlag = bFlag && false;
    }
    else if (txtPreferredUserName.hasClass('invalidUserName')) {
        txtPreferredUserName.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtPassword.val().trim() == '') {
        txtPassword.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtRepeatPassword.val().trim() == '') {
        txtRepeatPassword.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtVoiceMailPIN.val().trim() == '') {
        txtVoiceMailPIN.attr('required', true);
        bFlag = bFlag && false;
    }
    else if (txtVoiceMailPIN.val().trim().length < 4) {
        txtVoiceMailPIN.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtEmailAddress.val().trim() == '') {
        txtEmailAddress.attr('required', true);
        bFlag = bFlag && false;
    }
    else if (txtEmailAddress.hasClass('invalidEmail')) {
        txtEmailAddress.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtMobileNumber.val().trim() == '') {
        txtMobileNumber.attr('required', true);
        bFlag = bFlag && false;
    } else if (txtMobileNumber.hasClass("invalidMobileNumber")) {
        txtMobileNumber.attr('required', true);
        bFlag = bFlag && false;
    }

    else if (txtMobileNumber.val().trim().length < 10) {
        txtMobileNumber.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtSecretQuestion.val().trim() == '') {
        txtSecretQuestion.attr('required', true);
        bFlag = bFlag && false;
    }

    if (txtAnswer.val().trim() == '') {
        txtAnswer.attr('required', true);
        bFlag = bFlag && false;
    }
    if (!bFlag) {
        var borderColor = $(".TextBoxX").css("required");
        $(".form-control").css("border-color", borderColor);
    }
    return bFlag;
}

function ValidateBillingInfo() {

    ClearValidation();
    var bFlag = true;
    var isCreditCardEnable = true;
    var strCountry = "";
    var strState = "";
    var strMonth = "";
    var strYear = "";

    if ($("#creditCardDetails").is(":visible")) {
        isCreditCardEnable = true;
        txtNameOnCard = $("#txtNameOnCard");
        txtCardNumber = $("#txtCardNumber");
        txtCardSecurityNumber = $("#txtCardSecurityNumber");



        if (txtNameOnCard.val().trim() == '') {
            txtNameOnCard.attr('required', true);
            bFlag = bFlag && false;
        }
        else if (txtNameOnCard.hasClass("invalidNameOnCard")) {
            txtNameOnCard.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtCardNumber.val().trim() == '') {
            txtCardNumber.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtCardSecurityNumber.val().trim() == '') {
            txtCardSecurityNumber.attr('required', true);
            bFlag = bFlag && false;
        }

        //if ($("#ddlCardType ").val() == 0) {
        //    $('#ddlCardType').attr('required', true);
        //    bFlag = bFlag && false;
        //}

        if ($("#ddlMonth").val() == 0) {
            $('#ddlMonth').attr('required', true);
            bFlag = bFlag && false;
        }

        if ($("#ddlYear")[0].selectedIndex == 0) {
            $('#ddlYear').attr('required', true);
            bFlag = bFlag && false;
        }

        if ($("#sectionUSAAddress").is(":visible")) {
            txtAddressLine1 = $("#txtAddressLine1");
            txtAddressLine2 = $("#txtAddressLine2");
            txtCity = $("#txtCity");
            txtZipCode = $("#txtZipCode");

            if (txtAddressLine1.val().trim() == '') {
                txtAddressLine1.attr('required', true);
                bFlag = bFlag && false;
            }

            //if (txtAddressLine2.val().trim() == '') {
            //    txtAddressLine2.attr('required', true);
            //    bFlag = bFlag && false;
            //}
            if (txtCity.val().trim() == '') {
                txtCity.attr('required', true);
                bFlag = bFlag && false;
            } else if (txtCity.hasClass("invalidCity")) {
                txtCity.attr('required', true);
                bFlag = bFlag && false;
            }

            if ($("#ddlState").val() == 0) {
                $('#ddlState').attr('required', true);
                bFlag = bFlag && false;
            }

            if (txtZipCode.val().trim().length < 5) {
                txtZipCode.attr('required', true);
                bFlag = bFlag && false;
            }


        } else {
            txtInternationalStreet1 = $("#txtInternationalStreet1");
            txtInternationalStreet2 = $("#txtInternationalStreet2");
            txtInternationalCity = $("#txtInternationalCity");
            txtInternationalProvince = $("#txtInternationalProvince");
            txtInternationalPost = $("#txtInternationalPost");
            //txtInternationalZipcode = $("#txtInternationalZipcode");

            if (txtInternationalStreet1.val().trim() == '') {
                txtInternationalStreet1.attr('required', true);
                bFlag = bFlag && false;
            }

            //if (txtInternationalStreet2.val().trim() == '') {
            //    txtInternationalStreet2.attr('required', true);
            //    bFlag = bFlag && false;
            //}

            if (txtInternationalCity.val().trim() == '') {
                txtInternationalCity.attr('required', true);
                bFlag = bFlag && false;
            } else if (txtInternationalCity.hasClass("invalidInternationalCity")) {
                txtInternationalCity.attr('required', true);
                bFlag = bFlag && false;
            }

            if (txtInternationalProvince.val().trim() == '') {
                txtInternationalProvince.attr('required', true);
                bFlag = bFlag && false;
            } else if (txtInternationalProvince.hasClass("invalidInternationalProvince")) {
                txtInternationalProvince.attr('required', true);
                bFlag = bFlag && false;
            }

            if (txtInternationalPost.val().trim()=='') {
                txtInternationalPost.attr('required', true);
                bFlag = bFlag && false;
            }

         

            if ($("#ddlInternationalCountry").val() == 0) {
                $('#ddlInternationalCountry').attr('required', true);
                bFlag = bFlag && false;
            }

        }
    } else {
        isCreditCardEnable = false;
        txtNameOnAccount = $("#txtNameOnAccount");
        txtBankName = $("#txtBankName");
        txtRoutingTransmitNumber = $("#txtRoutingTransmitNumber");
        txtAccountNumber = $("#txtAccountNumber");
        txtReenterAccountNumber = $("#txtReenterAccountNumber");

        if (ValidateAccountNumberMatch() == false) {
            txtReenterAccountNumber.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtNameOnAccount.val().trim() == '') {
            txtNameOnAccount.attr('required', true);
            bFlag = bFlag && false;
        }
        else if (txtNameOnAccount.hasClass("invalidNameOnAccount")) {
            txtNameOnAccount.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtBankName.val().trim() == '') {
            txtBankName.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtRoutingTransmitNumber.val().trim() == '') {
            txtRoutingTransmitNumber.attr('required', true);
            bFlag = bFlag && false;
        }

        if (txtAccountNumber.val().trim() == '') {
            txtAccountNumber.attr('required', true);
            bFlag = bFlag && false;
        }
       
        if (txtReenterAccountNumber.val().trim() == '') {
            txtReenterAccountNumber.attr('required', true);
            bFlag = bFlag && false;
        }
    }

    if (bFlag) {
        if (isCreditCardEnable) {
            ValidateCreditCardInfo();
        }
        else {
            var isInValid = $("#txtRoutingTransmitNumber").hasClass("invalidRoutingCode");
            if (!isInValid) {
                ValidateACHInfo();
            }

        }
    }

    if (!bFlag) {
        var borderColor = $(".TextBoxX").css("required");
        $(".form-control").css("border-color", borderColor);

        var ddlBorderColor = $(".dropdownX").css("required");
        $(".form-control").css("border-color", ddlBorderColor);
    }



    return bFlag;
}

function ValidateTermsAndService() {
    var chkTns = $("#chkAgreeTOS");
    var isNotCheck = true;

    var chkValue = $("#chkAgreeTOS").attr("checked");
    if (chkValue == true) {
        $("#lblTOSError").remove();
        $("#chkAgreeTOS").removeAttr('required');
    } else {
        isNotCheck = false;
    }

    if (isNotCheck == false) {
        $("#lblTOSError").remove();
        $("#TOSError").addClass("lblInvalidTOS");
        var isInValid = $("#TOSError").hasClass("lblInvalidTOS");
        if (isInValid) {
            $("<label id='lblTOSError' for='txtAuthCode' class='errorLable' style='font-weight:bold;' generated='true'>Please Confirm Terms of Services.</label>").insertBefore(".lblInvalidTOS");
        }
        chkTns.attr('required', true);
        //ShowMessageDialog("Please select terms of services.");
        return false;
    }
    return true;
}

function ClearValidation() {
    $("input[type=text]").removeAttr('required');
    $("input[type=email]").removeAttr('required');
    $("input[type=password]").removeAttr('required');
    $("input[type=checkbox]").removeAttr('required');
}

function ValidateCreditCardInfo() {

    reqDataCreditCard = GetCreditCardData();
    reqDataBillingAddress = GetBillingData();

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/ValidateCreditCardInfo",
        data: "{creditCard:" + reqDataCreditCard + ",billingAddress:" + reqDataBillingAddress + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            hideLoadingPanel();
            if (response.d.IsSuccess == true) {
                isValidCreditCard = true;
                $("#lblInvalidCreditCardError").remove();
                $("#txtCardNumber").removeAttr("required");

                BtnNextClick(true);
            }
            else {
                if (response.d.ErrorMessage.trim() != '') {
                    $("#lblInvalidCreditCardError").remove();
                    $("#txtCardNumber").attr("required", "required");
                    $("#txtCardNumber").addClass("invalidCreditCard");
                    var isInValid = $("#txtCardNumber").hasClass("invalidCreditCard");
                    if (isInValid) {
                        $("<label id='lblInvalidCreditCardError' for='txtCardNumber' class='errorLable' generated='true'>" + response.d.ErrorMessage + "</label>").insertBefore(".invalidCreditCard");
                    }
                }
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

function ValidateACHInfo() {
    var routingNumber = $("#txtRoutingTransmitNumber").val();

    reqDataACH = GetACHData();
    reqDataBillingAddress = GetBillingData();
    

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/ValidateACHInfo",
        data: "{routingNumber: " + routingNumber + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            hideLoadingPanel();
            if (response.d.IsSuccess == true) {
                $("#lblInvalidRoutingCodeError").remove();
                $("#txtRoutingTransmitNumber").removeAttr("required");
                BtnNextClick(true);
            }
            else {
                $("#lblInvalidRoutingCodeError").remove();
                $("#txtRoutingTransmitNumber").attr("required", "required");
                $("#txtRoutingTransmitNumber").addClass("invalidRoutingCode");
                var isInValid = $("#txtRoutingTransmitNumber").hasClass("invalidRoutingCode");
                if (isInValid) {
                    $("<label id='lblInvalidRoutingCodeError' for='txtRoutingTransmitNumber' class='errorLable' generated='true'>Error in validate Routing Number info.</label>").insertBefore(".invalidRoutingCode");
                }
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

function GetCreditCardData() {
    var strCardNumber = $('#txtCardNumber').val();
    var cardNumber = strCardNumber.replace(/\s/g, '');
    var reqDataCreditCard = JSON.stringify({
        "Type": $("input[name='typeCard']:checked").val(),
        "NameOnCard": $('#txtNameOnCard').val(),
        "Number": cardNumber,
        "ExpMonth": $("#ddlMonth option:selected").val(),
        "ExpYear": $("#ddlYear option:selected").val(),
        "SecurityCode": $('#txtCardSecurityNumber').val()
    });
    return reqDataCreditCard;
}

function GetACHData() {
    var reqDataACHCard = JSON.stringify({
        "NameOnAccount": $('#txtNameOnAccount').val().trim(),
        "BankName": $("#txtBankName").val().trim(),
        "RoutingNumber": $("#txtRoutingTransmitNumber").val().trim(),
        "BankAccountNumber": $("#txtAccountNumber").val().trim()
    });
    return reqDataACHCard;
}

function GetBillingData() {
    if ($("#rblUnitedStatesAddress").attr('checked')) {
        var reqDataBillingAddress = JSON.stringify({
            "International": false,
            "Address1": $('#txtAddressLine1').val(),
            "Address2": $('#txtAddressLine2').val(),
            "City": $('#txtCity').val(),
            "State": $("#ddlState option:selected").val(),//$("#ddlState option:selected").text(),
            "Zip": $('#txtZipCode').val()
        });
    }
    else {
        var reqDataBillingAddress = JSON.stringify({
            "International": true,
            "Address1": $('#txtInternationalStreet1').val(),
            "Address2": $('#txtInternationalStreet2').val(),
            "City": $('#txtInternationalCity').val(),
            "State": $('#txtInternationalProvince').val(),
            "Zip": $('#txtInternationalPost').val(),
            "Country": $("#ddlInternationalCountry option:selected").text()
        });
    }
    return reqDataBillingAddress;
}

function GetCustomerInfoData() {

    var reqDataCustomerInfo = JSON.stringify({
        "FirstName": $("#txtFirstName").val().trim(),
        "LastName": $("#txtLastName").val().trim(),
        "Email": $("#txtEmailAddress").val().trim(),
        "WorkPhone": $("#txtMobileNumber").val().trim(),
    });
    return reqDataCustomerInfo;
}



function ValidateAuthorizationNumber() {
    if (authorizationNumber != $("#txtAuthCode").val().trim()) {
        productId = 0;
    }
    authorizationNumber = $("#txtAuthCode").val().trim();
    var sponsorId = $("#ddlSponsor option:selected").val();

    $.ajax({
        type: "POST",
        url: "SimpleSignUp.aspx/ValidateAuthorizationNumber",
        data: '{authorizationNumber: "' + authorizationNumber + '", sponsorId: "' + sponsorId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        beforeSend: function () {
            ShowLoadingPanel();
        },
        success: function (response) {
            hideLoadingPanel();
            if (response.d.IsSuccess == true) {
                $("#lblAuthCodeError").remove();
                $("#hdnLblSponsorProgramId").val(response.d.SponsorProgramId);
                SponsorPaymentApproch();
                //BtnNextClick(true);
            } else {
                $("#lblAuthCodeError").remove();
                $("#txtAuthCode").addClass("invalidAuthCode");
                var isInValid = $("#txtAuthCode").hasClass("invalidAuthCode");
                if (isInValid) {
                    $("<label id='lblAuthCodeError' for='txtAuthCode' class='errorLable' generated='true'>" + response.d.ErrorMessage + "</label>").insertBefore(".invalidAuthCode");
                    if (qsSponsorName != null) {
                        $("#lblGlobalError").show().html("Enter valid authorization number or activation number.");
                    }
                }
            }
        },
        failure: function (response) {
            hideLoadingPanel();
            alert(response.d);
        }
    });
}

/*********user name /Fname/Lname/ Email Validation********************************************************/
function ValidateEmail(email) {
    if (email == '') {
        return true;
    }
    var expr = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    return expr.test(email);
};

function ValidateUserNameExpression(userName) {
    if (userName.trim() == '') {
        return true;
    }
    var expr = /([a-zA-Z0-9.]{5,50})$/;
    return expr.test(userName);
}

function ValidateFirstNameLastName(name) {
    if (name.trim() == '') {
        return true;
    }
    var expr = /^([a-zA-Z\s]|\.|\,|-|\'|\&|\"|\(|\))+$/;
    return expr.test(name);
}

/********************************End**********************************************/

/*----------------Confirm Dialog---------------------------------------------------------*/
function ShowMessageDialog(msg) {
    $("#dialog-confirm").html(msg);
    // Define the Dialog and its properties.
    $("#dialog-confirm").dialog({
        resizable: false,
        modal: true,
        title: "Message",
        height: 150,
        width: 300,
        buttons: {
            "Ok": function () {
                $(this).dialog('close');
            }
        }
    });
}

function showConfrimDialog(msg) {
    $("#dialog-confirm").html(msg);
    // Define the Dialog and its properties.
    $("#dialog-confirm").dialog({
        resizable: false,
        modal: true,
        title: "Modal",
        height: 250,
        width: 400,
        buttons: {
            "Yes": function () {
                $(this).dialog('close');
                callback(true);
            },
            "No": function () {
                $(this).dialog('close');
                callback(false);
            }
        }
    });
}

function callback(value) {
    if (value) {
        alert("Confirmed");
    } else {
        alert("Rejected");
    }
}

function setBorderColor() {
    var borderColor = $(".TextBoxX").css("required");
    $(".form-control").css("border-color", borderColor);
}

