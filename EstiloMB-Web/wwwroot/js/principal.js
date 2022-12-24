function toggle(button) {

    if (button.style.color == "rgb(182, 132, 235)") {
        button.style.color = "rgb(255, 0, 0)";

    } else {
        button.style.color = "rgb(182, 132, 235)";
    }
}

function popup(element, template, callback) {

    let hasActive = template.className.includes('active');

    if (hasActive != true) {
        template.classList.add('active');

    } else {
        return template.classList.remove('active');
    }

    if (callback) {
        callback(element, template);
    }

}

function formatPrice(element) {

    let currency = element
    let value = element.innerHTML != "" ? element.innerHTML : element.value;

    if (isNaN(value)) {
        currency.value = "R$ 0,00";
        currency.innerHTML = "R$ 0,00";
        return;
    }

    let formatter = new Intl.NumberFormat("pt-BR", {
        style: "currency",
        currency: "BRL",
        minimumFractionDigits: 2,
    });

    let valorFormatado = formatter.format(value);

    currency.value = valorFormatado;
    currency.innerHTML = valorFormatado;

    return formatter.format(value);
}

(function ($mascara) {

    $mask.stringToDecimal = function (value, input) {

        let inputDecimal = input.closest('.label').querySelector('input[valorDecimal]');
        let inputText = input.closest('.label').querySelector('input[valorTexto]');
        let text = "";

        text = value.replace("R$", "");
        text = text.replace(",", ".");
        //console.log(text.match(/\./g).length)

        if ((text.match(/\./g) || []).length > 1) {
            text = text.replace("\.", "");
        }

        //console.log(inputDecimal)
        //console.log(inputText)
        inputDecimal.value = text;
        inputText.value = "R$ " + value;
    };

    $mask.valor = function (input) {
        let value = input.value;

        if (value != NaN) {
            value = value + '';
            value = parseInt(value.replace(/[\D]+/g, ''));
            value = value + '';
            value = value.replace(/([0-9]{2})$/g, ",$1");

            if (value.length > 6) {
                value = value.replace(/([0-9]{3}),([0-9]{2}$)/g, ".$1,$2");
            }

        } else {
            value = '00,00'
        }


        input.value = value;
        $mask.stringToDecimal(value, input);
    };

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
            template.querySelector("span").innerHTML = element.nextElementSibling.innerText;
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

function validateSameValue(select) {
    let tbody = select.closest("tbody");
    let selects = tbody.querySelectorAll("[name=" + select.name + "]");

    for (let i = 0; i < selects.length; i++) {
        if (selects[i].value === select.value && selects[i] !== select) {
            console.log('entrei')
            return false;
        }
    }

    return true;
};



