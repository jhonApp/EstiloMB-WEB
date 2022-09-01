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
