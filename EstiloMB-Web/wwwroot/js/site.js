function toggle(button) {

    if (button.style.color == "rgb(182, 132, 235)") {
        button.style.color = "rgb(255, 0, 0)";

    } else {
        button.style.color = "rgb(182, 132, 235)";
    }
}

function popup(element) {
    
    let popupBox = element.closest('section').querySelector('.popup-overlay');

    if (element.className == 'button-new') {
        popupBox.classList.add('active');

    } else {
        console.log("entrei primeiro")
        popupBox.classList.remove('active');
    }

}

(function ($mask) {

    
    $mask.moeda = function (element) {
        let value = element.value.replace(/\D/g, '');
        value = (value / 100).toFixed(2) + '';
        value = value.replace(".", ",");
        value = value.replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
        element.value = value;
    }

})(window.$mask = window.$mask || {});

validade = function (element) {
    console.log("entrei")
}




