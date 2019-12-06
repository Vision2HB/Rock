var initPaymentEntry = function () {
    function calculateFee(amount) {
        if (amount === 0) {
            return 0;
        }
        var fAmount = parseFloat(amount);
        var achRate = parseInt($('[id$="hfAchRate"]').val(), 10);
        var cardRate = parseFloat($('[id$="hfCardRate"]').val(), 10);
        var capAch = $('[id$="hfCapAch"]').val().toLowerCase() === 'true';
        var offsetAmount = ((fAmount + .50) / (1 - cardRate)) - fAmount;
        var isACH = $('[id$="hfPaymentTab"]').val() === 'ACH';

        if (capAch && (offsetAmount > achRate) && isACH) {
            offsetAmount = achRate;
        }

        return offsetAmount;
    }

    function updateText(amount) {
        var newAmount = amount.toFixed(2);
        console.log("Fee Amount: " + newAmount);
        $('.css-cover-parent > .control-label').text("Cover the $" + newAmount + " in processing fees");
        $('[id$="hfFeeAmount"]').val(newAmount);

        var total = $('[id$="hfTotalCost"]').val();
        var payingAmount = $('.amount-to-pay input').val();
        var remainingAmount = (total - payingAmount).toFixed(2);
        $('[id$="lRemainingDue"]').text( "$" + remainingAmount );
    }

    $('.amount-to-pay input').change(function () {
        var amount = calculateFee( $(this).val() );
        updateText(amount);
    });

    // Launching it once for POSTBACK Values
    var amount = 0;
    if ( $('.amount-to-pay input').length > 0 ) {
        amount = calculateFee($('.amount-to-pay input').val());
    }
    else {
        amount = calculateFee($('[id$="hfTotalCost"]').val());
    }

    updateText(amount);
};

$(initPaymentEntry);
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initPaymentEntry);
