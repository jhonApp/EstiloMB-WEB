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

        let input = element.parentNode.querySelector('[name = Valor]');
        let value = element.value.replace(/\D/g, '');
        
        value = (value / 100).toFixed(2) + '';

        //input.value será enviado para o bd
        input.value = parseFloat(value);

        value = value.replace(".", ",");
        value = value.replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
        element.value = "R$ " + value;
    }

})(window.$mask = window.$mask || {});

(function ($validate) {


    $validate.error = function (element) {
        console.log("entrei");
    }

})(window.$validate = window.$validate || {});

(function ($multiSelector) {

    //- Função de selecionar o checkbox caso a propriedade exista no dataArray
    $multiSelector.tagBind = function (element, dataArray, finderName, callback) {
        console.log("entrei")
        let checkbox = dataArray ? dataArray.find(e => e[finderName] == element.getAttribute('tag-bind')) : false;
        
        if (checkbox) {
            element.checked = true;

            if (callback) { eval(callback.replace(/\bthis\b/g, "element")) };
        }
    }

    //- Abre o dropdown
    $multiSelector.showCheckBoxes = function (element) {
        let checkboxes = element.parentNode.querySelector("#checkboxes");

        checkboxes.style.display == "none" ? checkboxes.style.display = "block" : checkboxes.style.display = "none";
    }

    $multiSelector.closeCheckBoxes = function (element) {
        let checkboxes = element.querySelector("#checkboxes");

        if (checkboxes.style.display != "none")
            checkboxes.style.display = "none";
    }

    //- Seleciona as tags
    $multiSelector.selecionando = function (element) {
        content = element.closest("[multiselect]");
        selectBox = content.querySelector("[select-btn]");

        let validacao = true;

        //- verifica se o checkbox do elemento está selecionado, senão, remove a tag
        if (!element.checked) {
            validacao = false;
            $multiSelector.remover(element.tag);
        }

        // verifica se já existe uma tag dentro da selectBox relacionada a opção selecionada
        selectBox.childNodes.forEach(function (child) {
            if (child.seletor == element) {
                validacao = false;
            }
        });

        //- Se ele passar pelas validações, ele cria a tag dentro do SelectBox e ao associa ao seletor para que um possa acessar o outro diretamente
        if (validacao) {

            template = document.importNode(selectBox.querySelector("#seletor").content, true).firstElementChild;
            template.querySelector("span").innerHTML = element.nextElementSibling.innerText;;
            template.seletor = element;
            element.tag = selectBox.appendChild(template);
        }
    }

    //- Remove as tags
    $multiSelector.remover = function (element) {
        selectBox = element.closest("[selectBox]");

        element.seletor.checked = false;
        element.seletor.tag == undefined;

        selectBox.removeChild(element);
    }

})(window.$multiSelector = window.$multiSelector || {});



