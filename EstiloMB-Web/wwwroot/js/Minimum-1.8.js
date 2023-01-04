"use strict";

if (!Array.prototype.remove) {
    Array.prototype.remove = function (from, to) {
        let rest = this.slice((to || from) + 1 || this.length);
        this.length = from < 0 ? this.length + from : from;
        return this.push.apply(this, rest);
    }
};

if (!String.prototype.format) {
    String.prototype.format = function () {
        let str = this.toString();
        if (arguments.length) {
            let t = typeof arguments[0];
            let key;
            let args = ("string" === t || "number" === t) ?
                Array.prototype.slice.call(arguments)
                : arguments[0];

            for (key in args) {
                str = str.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key]);
            }
        }

        return str;
    }
};

(function ($min) {
    // - Clears all children from [element].
    $min.clear = function (element) {
        while (element.lastElementChild) {
            element.removeChild(element.lastElementChild);
        }
    };

    // - Returns the first [element] where the [predicate] returns true.
    $min.find = function (element, predicate) {
        for (let i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType !== 1) { continue; }

            if (predicate(element.childNodes[i])) {
                return element.childNodes[i];
            }

            if (element.childNodes[i].hasChildNodes()) {
                let result = $min.find(element.childNodes[i], predicate);
                if (result) { return result; }
            }
        }
        return null;
    };

    // - Returns the first element that comes next in the DOM hierarchy where the [predicate] returns true.
    $min.findAfter = function (element, predicate) {
        let current = element;
        do {
            let search = current;
            while (search = search.nextElementSibling) {
                if (predicate(search)) { return search; }

                let result = $min.find(search, predicate);
                if (result) { return result; }
            }
        } while (current = current.parentNode);

        return null;
    };

    // - Returns the first element that comes before in the DOM hierarchy where the [predicate] returns true.
    $min.findBefore = function (element, predicate) {
        let current = element;
        do {
            let search = current;
            while (search = search.previousElementSibling) {
                if (predicate(search)) { return search; }

                let result = $min.find(search, predicate);
                if (result) { return result; }
            }
        } while (current = current.parentNode);

        return null;
    };

    // - Executes a [delegate] function for every child within [element].
    $min.forEach = function (element, delegate) {
        for (let i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType !== 1) { continue; }

            delegate(element.childNodes[i]);

            if (element.childNodes[i].hasChildNodes()) {
                $min.forEach(element.childNodes[i], delegate);
            }
        }
    };

    // - Returns an object containing the [element] coordinates/dimensions to the page or to a [relativeParent] (optional).
    // { top: int, bottom: int, left: int, right: int, width: int, height: int }
    $min.getPosition = function (element, relativeParent) {
        let position = {
            top: 0,
            bottom: 0,
            left: 0,
            right: 0,
            width: element.offsetWidth,
            height: element.offsetHeight
        };

        let current = element;
        if (current.parentNode === document.body) {
            position.left = current.offsetLeft;
            position.top = current.offsetTop;
        }
        else if (current.offsetParent) {
            do {
                if (relativeParent && current === relativeParent) { break; }
                position.left += current.offsetLeft;
                position.top += current.offsetTop;
            } while (current = current.offsetParent);
        }

        position.right = window.innerWidth - position.width - position.left;
        position.bottom = window.innerHeight - position.height - position.top;

        return position;
    };

    // - Changes the display value for all children within [element] whose @toggle attribute contains [value].
    // If [group] is present, the attribute checked will be @toggle-[group] instead.
    $min.toggle = function (element, value, group) {
        for (let i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType !== 1) { continue; }

            let toggle = element.childNodes[i].getAttribute("toggle" + (group ? "-" + group : ""));
            if (toggle) {
                element.childNodes[i].style.display = toggle.includes(value) ? element.childNodes[i].getAttribute("toggle-display") || "inline-block" : "none";
                //element.childNodes[i].style.display = new RegExp("\\b" + value + "\\b", "g").test(value) ? element.childNodes[i].getAttribute("toggle-display") || "inline-block" : "none";                
            }

            if (element.childNodes[i].hasChildNodes()) {
                $min.toggle(element.childNodes[i], value, group);
            }
        }
    };

    // - Toggle a CSS [className] on the closest [selector] from [element].
    $min.toggleClass = function (element, selector, className) {
        let target = element.closest(selector);

        if (target.classList.contains(className)) {
            target.classList.remove(className);
        }
        else {
            target.classList.add(className);
        }
    };

    // - Toggles the display (show/hide) for all elements within the closest [selector] from [element] whose @toggle attribute matches [value].
    // The [name] is a value to enable multiple selections.
    $min.toggleElements = function (element, selector, name, value) {
        let container = selector ? element.closest(selector) : element;
        if (!container.toggles) { container.toggles = {}; }

        if (value === undefined || value === "" || value === null) {
            delete container.toggles[name];
        }
        else {
            container.toggles[name] = value;
        }

        //console.log(container.toggles);
        toggleElements(container, container.toggles);
    };

    var toggleElements = function (element, values) {
        for (let i = 0; i < element.childNodes.length; i++) {
            if (element.childNodes[i].nodeType !== 1) { continue; }

            let toggle = element.childNodes[i].getAttribute("toggle");
            if (toggle) {
                let isVisible = true;
                for (const value of Object.values(values)) {
                    if (!toggle.includes(value)) { isVisible = false; break; }
                }

                element.childNodes[i].style.display = isVisible ? element.childNodes[i].getAttribute("toggle-display") || "inline-block" : "none";
            }

            if (element.childNodes[i].hasChildNodes()) {
                toggleElements(element.childNodes[i], values);
            }
        }
    };

    // - Formats a [input] value by a [pattern] that uses numbers and letters as placeholders.
    // Ex: '000.00' or 'A.AA-AA'
    $min.pattern = function (input, pattern) {
        if (!pattern) { return; }

        let selectionStart = input.selectionStart;
        let caret = input.value.length !== input.selectionStart;
        let value = input.value.replace(/[^\w]/g, '');

        for (let v = 0, p = 0; v < value.length; v++, p++) {
            if (p > pattern.length) {
                value = value.substr(0, pattern.length);
                break;
            }
            else if (/^[0-9]$/.test(pattern[p])) {
                if (!/^[0-9]$/.test(value[v])) {
                    value = value.substr(0, v);
                    break;
                }
            }
            else if (/^[a-zA-Z]$/.test(pattern[p])) {
                if (!/^[a-zA-Z]$/.test(value[v])) {
                    value = value.substr(0, v);
                    break;
                }
            }
            else {
                value = value.substr(0, v) + pattern[p] + value.substr(v, value.length);
                //v++; p++;
                if (selectionStart === p) { selectionStart++; }
            }
        }

        input.value = value;

        if (caret) {
            input.selectionStart = input.selectionEnd = selectionStart;
        }
    };

    // - Formats a [elementOrValue] by it's @data-format attribute if it's a element, or by [optionalFormat] if it's a value.
    // Ex: 
    // <input type="text" data-format="dd/MM/yyyy HH:mm" />
    // <input type="text" data-format="3.0,2?" /> (3 thousands separator, comma decimal separator, 2 decimal places, optional)
    $min.format = function (elementOrValue, optionalFormat) {
        let isElement = elementOrValue instanceof Element || elementOrValue instanceof HTMLDocument;

        let format = optionalFormat ? optionalFormat : isElement && elementOrValue.hasAttribute("data-format") ? elementOrValue.getAttribute("data-format") : "";
        if (!format) { return elementOrValue; }

        let selectionStart = 0;
        let offset = 0;
        let caret = 0;

        // - Element event
        if (isElement) {
            selectionStart = elementOrValue.selectionStart;
            caret = elementOrValue.value ? elementOrValue.value.length !== elementOrValue.selectionStart : false;
            if (caret) {
                offset = (elementOrValue.value.match(/\./g) || []).length;
            }
        }

        let result = "";

        if (/[0-9]/.test(format[0])) { // - Number            
            let formats = format.replace(/([1-9]*)([^0-9]*)0([^0-9]*)([0-9]*)(\??)/g, "$1|$2|$3|$4|$5").split("|");
            //console.log("thousands: " + formats[0]);
            //console.log("thousands character: " + formats[1]);        
            //console.log("decimal character: " + formats[2]);
            //console.log("decimal places: " + formats[3]);
            //console.log("omit formats: " + formats[4]);

            let number = isElement ? (elementOrValue.value || elementOrValue.innerHTML).toString() : elementOrValue.toString();
            let isNegative = number.indexOf("-") > -1;
            let isCommaLast = !isElement ? false : number.length === 0 ? false : number.lastIndexOf(formats[2]) === number.length - 1;

            let decimalPlaces = !isElement && number.indexOf(".") > -1 ? number.length - number.indexOf(".") - 1 : number.indexOf(formats[2]) > -1 ? number.length - number.lastIndexOf(formats[2]) - 1 : - 1;
            if (isElement && !formats[4] && decimalPlaces < formats[3]) { decimalPlaces = formats[3]; number = number.padStart(formats[3], "0"); }
            if (isElement && decimalPlaces > formats[3]) { decimalPlaces = formats[3]; }

            // - Cleaning number.            
            number = number.replace(/[^\d]/g, '');
            if (number !== "") {
                // - Adding decimal places.
                if (decimalPlaces > -1) {
                    number = !isElement && number.length - decimalPlaces <= 0 ? number + "." : number.substr(0, number.length - decimalPlaces) + "." + number.substr(number.length - decimalPlaces);
                }

                //console.log(number + ", " + decimalPlaces);

                number = isNegative ? -number : +number;

                if (!formats[4]) {
                    number = number.toFixed(formats[3]);
                }

                if (formats[2]) {
                    number = number.toString().replace(".", formats[2]);
                }

                number = number.toString();

                if (formats[0]) {
                    let decimals = "";
                    if (formats[2] && number.indexOf(formats[2]) > -1) {
                        decimals = number.substr(number.indexOf(formats[2]));
                        number = number.substr(0, number.indexOf(formats[2]));
                    }

                    number = number.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1" + formats[1]) + decimals;
                }

                if (isCommaLast && decimalPlaces < 0) {
                    number += formats[2];
                }
            }

            result = number;
        }
        else { // - Date
            // - TODO: A Miracle Here
            //let years = Math.floor(value / 31536000000);
            //let months = Math.floor(value / 2592000000);
            //let days = Math.floor(value / 86400000);
            //let hours = Math.floor(value % 86400000) / 3600000;
            //let minutes = Math.floor(value % 86400000) % 3600000 / 60000;
            //let seconds = Math.floor(value % 86400000) % 3600000 % 60000 / 1000;

            //let data = new Date(years, months, days, hours, minutes, seconds);
            //var value = new Date(date.replace(/-/g, '\/').replace(/T.+/, ''));
            //let data = new Date(date.replace(/-/g, '\/').replace(/T.+/, ''));
            let data = typeof elementOrValue === Date ? elementOrValue : new Date(elementOrValue);
            if (isNaN(data)) { return data; }

            result = format.replace("yyyy", data.getFullYear())
                .replace("yy", data.getFullYear().toString().substr(2))
                .replace("MM", ("00" + (data.getMonth() + 1)).toString().substr(-2))
                .replace("dd", ("00" + data.getDate()).toString().substr(-2))
                .replace("HH", ("00" + data.getHours()).toString().substr(-2))
                .replace("mm", ("00" + data.getMinutes()).toString().substr(-2))
                .replace("ss", ("00" + data.getSeconds()).toString().substr(-2));
        }

        // - Element event
        if (isElement) {
            switch (elementOrValue.nodeName.toUpperCase()) {
                case "INPUT":
                case "SELECT":
                case "TEXTAREA":
                    {
                        elementOrValue.value = result;
                        break;
                    }
                default:
                    {
                        elementOrValue.innerHTML = result;
                        break;
                    }
            }

            if (caret && elementOrValue.selectionEnd) {
                elementOrValue.selectionStart = elementOrValue.selectionEnd = selectionStart + (elementOrValue.value.match(/\./g) || []).length - offset;
            }
        }

        return result;
    };

    // - Returns the raw value of a [element] according to it's @data-format attribute.
    $min.unformat = function (element) {
        if (!element.hasAttribute("data-format")) { return element.value || element.innerHTML; }

        let format = element.getAttribute("data-format").replace(/([^\{]*)\{(.*)\}(.*)/g, "$2");
        let value = element.value || element.innerHTML;

        if (!format) {
            // - RAW
            return value;
        }
        else if (/[0-9]/.test(format[0])) {
            // - NUMBER
            let formats = format.replace(/([1-9]*)([^0-9]*)0([^0-9]*)([0-9]*)(\??)/g, "$1|$2|$3|$4|$5").split("|");
            //console.log("thousands: " + formats[0]);
            //console.log("thousands character: " + formats[1]);        
            //console.log("decimal character: " + formats[2]);
            //console.log("decimal places: " + formats[3]);
            //console.log("omit formats: " + formats[4]);        

            if (value === "-") { return 0; }

            if (formats[1]) {
                value = value.replace(new RegExp(formats[1].replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1"), "g"), "");
            }

            if (formats[2] && formats[2] !== ".") {
                return value.replace(new RegExp(("([^\\d" + formats[2] + "])").replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1"), "g"), "").replace(formats[2], ".");
            }

            return value.replace(/[^\d\-\.]/g, '');
        }
        else {
            if (!value) { return 0; }

            // - DATE
            let year = format.indexOf("yyyy") > -1 ? value.substr(format.indexOf("yyyy"), 4) : format.indexOf("yy") > -1 ? value.substr(format.indexOf("yy"), 2) : "";
            let month = format.indexOf("MM") > -1 ? value.substr(format.indexOf("MM"), 2) : "";
            let day = format.indexOf("dd") > -1 ? value.substr(format.indexOf("dd"), 2) : "";
            let hour = format.indexOf("HH") > -1 ? value.substr(format.indexOf("HH"), 2) : "";
            let min = format.indexOf("mm") > -1 ? value.substr(format.indexOf("mm"), 2) : "";
            let sec = format.indexOf("ss") > -1 ? value.substr(format.indexOf("ss"), 2) : "";

            return new Date(year || 0, month ? month - 1 : 0, day || 0, hour || 0, min || 0, sec || 0);
        }
    };

    // - Returns an object containing all values from elements with @name from [element] and store it on [data] (optional).
    // Ex: <div>
    //          <input type="text" name="Name" value="Coelho" />
    //          <div data-object="Dog">
    //                  <input type="text" name="Name" value="Tchula" />
    //          </div>
    //          <div data-array="Cats">
    //              <div>
    //                  <input type="text" name="Name" value="Miungo">
    //                  <input type="text" name="Name" value="Pituca">
    //                  <input type="text" name="Name" value="Ababa" data-ignore="true">
    //              </div>
    //          </div>
    //     </div>
    // Produces: {
    //      Name: "Coelho",
    //      Dog: {
    //          Name: "Tchula"
    //      },
    //      Cats: [
    //          { Name: "Miungo" },
    //          { Name: "Pituca" }
    //      ]
    //  }
    $min.read = function (element, data) {
        if (!data) { data = {}; }

        if (element.hasAttribute && element.hasAttribute("data-ignore")) { return data; }

        let property, members;

        if (element.name) {
            let value = "";
            switch (element.nodeName.toUpperCase()) {
                case "INPUT":
                    {
                        switch (element.type) {
                            case "checkbox":
                                //{ value = element.checked; break; }
                                { if (element.checked) { value = element.getAttribute("value") || true; } break; }
                            case "radio":
                                { if (element.checked) { value = element.value; } break; }
                            case "file":
                                {
                                    if (element.files[0]) {
                                        let reader = new FileReader();
                                        reader.onload = function () {
                                            let dataURL = reader.result;
                                            //let filename = input.value.indexOf('\\') > -1 ? input.value.substring(input.value.lastIndexOf('\\') + 1) : input.value;

                                            value = dataURL.substring(dataURL.indexOf(',') + 1);
                                        };
                                        reader.readAsDataURL(element.files[0]);
                                    }
                                    break;
                                }
                            default:
                                { value = element.value ? $min.unformat(element) : ""; break; }
                        }
                        break;
                    }
                //case "TEXTAREA":
                //case "SELECT":
                default:
                    { value = element.value; break; }
            }

            if (value !== "") {
                property = element.name.replace(/\[/g, ".").replace(/\]/g, "");

                let current = false;
                do {
                    current = data;

                    let currentArray = false;
                    if (current.constructor === Array) {
                        currentArray = current;
                        if (current.length === 0) { current.push({}); }
                        current = current[current.length - 1];
                    }

                    members = property.split(".");
                    for (let i = 1; i < members.length; i++) {
                        if (!members[i - 1]) { continue; }
                        if (current[members[i - 1]] === undefined) { current[members[i - 1]] = isNaN(members[i]) ? {} : []; }
                        current = current[members[i - 1]];
                    }
                    property = property.substring(property.lastIndexOf(".") + 1);

                    if (currentArray && current[property] !== undefined) {
                        currentArray.push({});
                        current = false;
                    }
                } while (current === false);

                current[property] = value;
            }
        }

        if (element.hasChildNodes()) {
            if (element.hasAttribute("data-object") || element.hasAttribute("data-array")) {
                if (data.constructor === Array) {
                    if (!data[data.length - 1]) { data.push({}); }
                    data = data[data.length - 1];
                }

                property = (element.getAttribute("data-object") || element.getAttribute("data-array")).replace(/\[/g, ".").replace(/\]/g, "");
                members = property.split(".");

                for (let i = 1; i < members.length; i++) {
                    if (!members[i - 1]) { continue; }
                    if (data[members[i - 1]] === undefined) { data[members[i - 1]] = isNaN(members[i]) ? {} : []; }
                    data = data[members[i - 1]];
                }
                property = property.substring(property.lastIndexOf(".") + 1);
                data[property] = element.hasAttribute("data-object") ? data[property] || {} : data[property] && data[property].constructor === Array ? data[property] : [];
                data = data[property];
            }

            for (let i = 0; i < element.childNodes.length; i++) {
                if (element.childNodes[i].nodeType !== 1) { continue; }

                $min.read(element.childNodes[i], data);
            }
        }

        return data;
    };

    // - Writes to all children within [element] the values from [data] properties matching the @data-bind or @data-array attributes.
    // If [group] is present, will only match those with a matching @data-group value.
    // Object: {
    //    Name: "Coelho",
    //    Dog: {
    //        Name: "Tchula"
    //    },
    //    Cats: [
    //        { Name: "Miungo" },
    //        { Name: "Pituca" }
    //    ]
    // }
    // <div>
    //    <input type="text" name="Name" value="Coelho" />
    //    <div data-object="Dog">
    //        <input type="text" name="Name" value="Tchula" />
    //    </div>
    //    <div data-array="Cats">
    //        <template>
    //            <div>
    //                <input type="text" name="Name" value="Miungo" onbind="console.log(data)">
    //                <input type="text" name="Name" value="Pituca" data-bind-html="false">
    //                <input type="text" name="Name" value="Ababa">
    //            <div>
    //        </template>
    //    </div>
    // </div>
    $min.bind = function (element, data, group) {
        let property, member, value;
        if (element.hasAttribute && (!group && !element.hasAttribute("data-group") || group && element.getAttribute("data-group") === group)) {
            if (data && (element.hasAttribute("data-bind") || element.hasAttribute("data-array"))) {
                property = element.getAttribute("data-bind") || element.getAttribute("data-array") || "";
                property = property.replace(/\[/g, ".").replace(/\]/g, "");

                let current = data;
                do {
                    member = property.indexOf(".") > -1 ? property.substring(0, property.indexOf(".")) : property;
                    if (current[member] === undefined || current[member] === null) { current = null; break; }
                    current = current[member];
                    property = property.substring(member.length + 1);
                } while (property);

                if (element.hasAttribute("data-array") && (element.template = element.template || $min.find(element, function (e) { return e.nodeName.toUpperCase() === "TEMPLATE"; }))) {
                    //element.template = element.template || element.querySelector("template");
                    //if (!element.template) { return console.log("No <template> found in the <" + element.nodeName + " data-array=\"" + element.getAttribute("data-array") + "\"> element."); }

                    $min.clear(element);
                    if (current && current.constructor === Array) {
                        for (let i = 0; i < current.length; i++) {
                            let template = document.importNode(element.template.content, true);
                            let item = current[i];
                            $min.bind(template, item, group);

                            element.appendChild(template);
                        }
                    }

                    if (element.hasAttribute("onbind")) {
                        eval(element.getAttribute("onbind").replace(/\bthis\b/g, "element"));
                    }

                    return;
                }
                else if (!element.hasAttribute("data-array")) {
                    value = current;
                    if (value === null || value === "null") { value = ""; }

                    switch (element.nodeName.toUpperCase()) {
                        case "INPUT":
                            {
                                switch (element.type) {
                                    case "checkbox":
                                        { element.checked = value === true || value.toString() === element.value ? true : false; break; }
                                    case "radio":
                                        { element.checked = value.toString() === element.value ? true : false; break; }
                                    case "date":
                                        { element.value = value === "" ? "" : $min.format(value, "yyyy-MM-dd"); break; }
                                    default:
                                        { element.value = value === "" ? "" : $min.format(value, element.getAttribute("data-format")); break; }
                                }
                                break;
                            }
                        case "SELECT":
                            { if (element.querySelector("option[value=\"" + value + "\"]")) { element.value = value; } break; }
                        case "TEXTAREA":
                            { element.value = value === "" ? "" : $min.format(value, element.getAttribute("data-format")); break; }
                        default:
                            { element.innerHTML = value === "" ? "" : $min.format(value, element.getAttribute("data-format")); break; }
                    }
                }
            }

            if (element.hasAttribute("onbind")) {
                eval(element.getAttribute("onbind").replace(/\bthis\b/g, "element"));
            }
        }

        if (element.hasChildNodes()) {
            if (element.hasAttribute && element.hasAttribute("data-object")) {
                property = element.getAttribute("data-object");
                property = property.replace(/\[/g, ".").replace(/\]/g, "");

                do {
                    member = property.indexOf(".") > -1 ? property.substring(0, property.indexOf(".")) : property;
                    if (data[member] === undefined || data[member] === null) { data = {}; break; }
                    data = data[member];
                    property = property.substring(member.length + 1);
                } while (property);
            }

            for (let i = 0; i < element.childNodes.length; i++) {
                if (element.childNodes[i].nodeType !== 1) { continue; }
                $min.bind(element.childNodes[i], data, group);
            }
        }
    };

    // - Returns true is all children within [element] are valid, and writes the error message to a <validation> sibling element if present.
    // Ex:
    // <label>
    //    <input type="text"
    //           required="required"
    //           required-error="This field is required."
    //           onvalidation="false"
    //           onvalidation-error="This is always false.">
    //    <validation></validation>
    // </label>    
    var lastInvalidElement;
    $min.validate = function (element, focusInvalidElement) {
        let name = element.nodeName.toUpperCase();
        if (name === "INPUT" || name === "SELECT" || name === "TEXTAREA") {
            if (element.disabled && element.hasAttribute("onvalid")) {
                eval(element.getAttribute("onvalid").replace("this", "element"));
                return true;
            }

            let isValid = true;
            let errorType = "";
            let min, max, value;
            switch (element.type.toUpperCase()) {
                case "CHECKBOX":
                    {
                        if (element.hasAttribute("required") && element.checked === false) {
                            isValid = false;
                            errorType = "required";
                        }
                        break;
                    }
                case "RADIO":
                    {
                        // - TODO: Improve, this checks once per radio button regardless if the group's been checked already.
                        if (element.hasAttribute("required")) {
                            isValid = false;
                            for (let i = 0; i < element.form.length; i++) {
                                if (!element.form[i].type.toUpperCase() === "RADIO" || element.form[i].name !== element.name) { continue; }
                                if (element.form[i].checked) {
                                    isValid = true;
                                    break;
                                }
                            }

                            if (!isValid) {
                                errorType = "required";
                            }
                        }
                        break;
                    }
                case "NUMBER":
                    {
                        if (element.hasAttribute("required") && !element.value) {
                            isValid = false;
                            errorType = "required";
                        }

                        if (element.hasAttribute("min") && element.value) {
                            min = parseFloat(element.getAttribute("min"));
                            value = parseFloat(element.value);
                            if (isNaN(min) || isNaN(value) || value < min) {
                                isValid = false;
                                errorType = "min";
                            }
                        }

                        if (element.hasAttribute("max") && element.value) {
                            max = parseFloat(element.getAttribute("max"));
                            value = parseFloat(element.value);
                            if (isNaN(max) || isNaN(value) || value > max) {
                                isValid = false;
                                errorType = "max";
                            }
                        }
                        break;
                    }
                case "DATE":
                    {
                        if (element.hasAttribute("required") && !element.value) {
                            isValid = false;
                            errorType = "required";
                        }

                        if (element.hasAttribute("min") && element.value) {
                            min = Date.parse(element.getAttribute("min"));
                            value = Date.parse(element.value);
                            if (isNaN(min) || isNaN(value) || value < min) {
                                isValid = false;
                                errorType = "min";
                            }
                        }

                        if (element.hasAttribute("max") && element.value) {
                            max = Date.parse(element.getAttribute("max"));
                            value = Date.parse(element.value);
                            if (isNaN(max) || isNaN(value) || value > max) {
                                isValid = false;
                                errorType = "max";
                            }
                        }
                        break;
                    }
                default:
                    {
                        if (element.hasAttribute("required") && !element.value) {
                            isValid = false;
                            errorType = "required";
                        }

                        if (element.hasAttribute("pattern") && element.value) {
                            let regex = new RegExp(element.getAttribute("pattern"));
                            if (!regex.test(element.value)) {
                                isValid = false;
                                errorType = "pattern";
                            }
                        }

                        if (element.hasAttribute("minlength") && element.value) {
                            min = parseFloat(element.getAttribute("minlength"));
                            if (isNaN(min) || element.value.length < min) {
                                isValid = false;
                                errorType = "minlength";
                            }
                        }

                        if (element.hasAttribute("maxlength") && element.value) {
                            max = parseFloat(element.getAttribute("maxlength"));
                            if (isNaN(max) || element.value.length > max) {
                                isValid = false;
                                errorType = "maxlength";
                            }
                        }
                        break;
                    }
            }

            if (isValid && element.hasAttribute("onvalidation")) {
                if (!Function("return " + element.getAttribute("onvalidation")).call(element)) {
                    isValid = false;
                    errorType = "onvalidation";
                }
            }

            if (isValid) {
                element.classList.remove("error");
                element.classList.add("valid");

                if (element.hasAttribute("onvalid")) {
                    Function(element.getAttribute("onvalid")).call(element);
                }

                if (element.validation === undefined) {
                    element.validation = $min.find(element.parentNode, function (e) { return e.getAttribute("data-group") == "error"; }) || false;
                }

                if (element.validation) {
                    element.validation.innerHTML = "";
                }
            }

            if (!isValid) {
                element.classList.remove("valid");
                element.classList.add("error");

                if (element.hasAttribute("onerror")) {
                    Function("type", element.getAttribute("onerror")).call(element, errorType);
                }

                if (element.validation === undefined) {
                    element.validation = $min.find(element.parentNode, function (e) { return e.getAttribute("data-group") == "error"; }) || false;
                }

                if (element.validation) {
                    element.validation.innerHTML = element.getAttribute(errorType + "-error") || "Unspecified validation message for \"" + errorType + "\".";
                }

                lastInvalidElement = element;
            }

            return isValid;
        }

        let status = true;
        if (element.hasChildNodes()) {
            for (let i = 0; i < element.childNodes.length; i++) {
                if (element.childNodes[i].nodeType !== 1) { continue; }
                status = !$min.validate(element.childNodes[i], focusInvalidElement) ? false : status;
            }
        }

        if (focusInvalidElement === true && status === false && lastInvalidElement) {
            lastInvalidElement.scrollIntoView({
                behavior: "smooth",
                block: "center",
                inline: "center"
            });
        }

        return status;
    };

    // - Evaluates all expressions from the @calc attribute for all children within [element].
    // If [group] is present only the @calc from a matching @calc-group value will be evaluated.
    // Ex:
    // <div id="teste">
    //    <div>
    //        <div calc-context="produto">
    //            <input type="number" calc-name="Valor" />
    //            <input type="number" calc-name="ValorDolar" />
    //            <input type="number" readonly="readonly" calc="Valor + ValorDolar" />
    //            <input type="time" calc-name="Time" calc-type="time" value="04:00">
    //        </div>

    //        <div calc-context="produto">
    //            <input type="number" calc-name="Valor" />
    //            <input type="number" calc-name="ValorDolar" data-format="{3.0,2}" />
    //            <input type="number" readonly="readonly" calc="Valor + ValorDolar" />
    //            <input type="time" calc-name="Time" calc-type="time" value="03:00">
    //        </div>    
    //    </div>
    //    <input type="number" calc="avg(produto.Valor)" calc-order="1" />
    //    <input type="number" calc="hours(sum(produto.Time))" calc-order="1" />
    // </div>
    $min.calculate = function (element, group) {
        let data = {};
        let calcs = [];

        getCalcElements(element, data, calcs, group);

        for (let o = 0; o < calcs.length; o++) { // - Ordering                
            for (let i = 0; i < calcs[o].length; i++) { // - Elements
                let context = calcs[o][i].context;
                let element = calcs[o][i].element;
                let calc = element.getAttribute("calc");

                let functions = [];
                let count = 0;
                while (/(sum|avg)\([^\)]*\)/.test(calc)) {
                    functions.push(calc.match(/(sum|avg)\([^\)]*\)/)[0].replace(/\(([^\)]*)\)/, "(context, '$1')"));
                    calc = calc.replace(/(sum|avg)\(([^\)]*)\)/, "$$" + count++);
                }

                for (let property in context) {
                    if (context[property].constructor === Array) { continue; }

                    let value = 0;
                    let current = context[property];
                    if (current.type === "time" || current.getAttribute("calc-type") === "time") {
                        value = timeInMilliseconds(current.value || current.innerHTML || 0);
                    }
                    else if (current.type === "date" || current.type === "datetime" || current.type === "datetime-local" || current.type === "month" || current.getAttribute("calc-type") === "date") {
                        value = dateInMilliseconds(current.value || current.innerHTML || 0);
                    }
                    else if (current.hasAttribute("data-format")) {
                        value = $min.unformat(current) || 0;
                    }
                    else {
                        value = context[property].value || context[property].innerHTML || 0;
                    }

                    calc = calc.replace(new RegExp("\\b" + property + "\\b", "g"), value);
                }

                for (let j = 0; j < functions.length; j++) {
                    calc = calc.replace("$" + j, "parseFloat(" + functions[j] + ")");
                }

                let result = 0;
                try {
                    result = eval(calc.replace("this", "element"));
                }
                catch (error) {
                    console.log("Error when evaluating the expression " + calc + ": " + error);
                }

                if (!isNaN(result)) {
                    result = $min.format(result, element.getAttribute("data-format"));
                }

                switch (element.nodeName.toUpperCase()) {
                    case "INPUT":
                    case "SELECT":
                    case "TEXTAREA":
                        {
                            element.value = result;
                            break;
                        }
                    default:
                        {
                            element.innerHTML = result;
                            break;
                        }
                }

                if (element.hasAttribute("oncalc")) {
                    eval(element.getAttribute("oncalc").replace("this", "element"));
                }
            }
        }
    };

    var getCalcElements = function (element, data, calcs, group) {
        if (!group || element.getAttribute("calc-group") === group) {
            if (element.hasAttribute("calc")) {
                if (!calcs[element.getAttribute("calc-order") || 0]) {
                    calcs[element.getAttribute("calc-order") || 0] = [];
                }

                calcs[element.getAttribute("calc-order") || 0].push({
                    element: element,
                    context: data
                });

                //calcs.push({
                //    element: element,
                //    context: data
                //});
            }

            let name = element.getAttribute("calc-name");
            if (name) {
                data[name] = element;
            }
        }

        if (element.hasChildNodes()) {
            if (element.hasAttribute("calc-context")) {
                let context = element.getAttribute("calc-context");
                if (!data[context]) { data[context] = []; }

                let item = {};
                data[context].push(item);
                data = item;
            }

            for (let i = 0; i < element.childNodes.length; i++) {
                if (element.childNodes[i].nodeType !== 1) { continue; }
                getCalcElements(element.childNodes[i], data, calcs, group);
            }
        }
    };

    var forEachProperty = function (data, property, delegate) {
        let member = "";
        if (property.indexOf(".") > -1) {
            member = property.substring(0, property.indexOf("."));
            property = property.substring(property.indexOf(".") + 1);
        }
        else {
            member = property;
        }

        if (!data[member]) {
            delegate(0);
        }
        else if (data[member].constructor === Array) {
            for (let i = 0; i < data[member].length; i++) {
                forEachProperty(data[member][i], property, delegate);
            }
        }
        else {
            delegate(data[member] || 0);
        }
    };

    var sum = function (context, property) {
        let total = 0;
        forEachProperty(context, property, function (element) {
            if (!element) { return; }

            let value = 0;
            if (element.type === "time" || element.getAttribute("calc-type") === "time") {
                value = timeInMilliseconds(element.value || element.innerHTML || 0);
            }
            else if (element.type === "date" || element.getAttribute("calc-type") === "date" ||
                element.type === "datetime" || element.getAttribute("calc-type") === "datetime" ||
                element.type === "datetime-local" || element.getAttribute("calc-type") === "datetime-local" ||
                element.type === "month" || element.getAttribute("calc-type") === "month") {
                value = dateInMilliseconds(element.value || element.innerHTML || 0);
            }
            else if (element.hasAttribute("data-format")) {
                value = $min.unformat(element) || 0;
            }
            else {
                value = element.value || element.innerHTML || 0;
            }

            total += parseFloat(value || 0);
        });

        return total;
    };

    var avg = function (context, property) {
        var total = 0;
        var count = 0;
        forEachProperty(context, property, function (element) {
            if (!element) { return; }

            let value = 0;
            if (element.type === "time" || element.getAttribute("calc-type") === "time") {
                value = timeInMilliseconds(element.value || element.innerHTML || 0);
            }
            else if (element.type === "date" || element.getAttribute("calc-type") === "date" ||
                element.type === "datetime" || element.getAttribute("calc-type") === "datetime" ||
                element.type === "datetime-local" || element.getAttribute("calc-type") === "datetime-local" ||
                element.type === "month" || element.getAttribute("calc-type") === "month") {
                value = dateInMilliseconds(element.value || element.innerHTML || 0);
            }
            else if (element.hasAttribute("data-format")) {
                value = $min.unformat(element) || 0;
            }
            else {
                value = element.value || element.innerHTML || 0;
            }

            total += parseFloat(value || 0);
            count++;
        });

        return total / (count === 0 ? 1 : count);
    };

    var hours = function (milliseconds) {
        if (!milliseconds) { return "00:00"; }

        //let days = Math.floor(value / 86400000);
        let hours = Math.floor(milliseconds / 3600000);
        let minutes = Math.abs(Math.floor(milliseconds / 60000 % 60));
        //let seconds = Math.floor(value / 1000 % 60);

        return hours.toString().padStart(2, '0') + ":" + minutes.toString().padStart(2, '0');
    };

    var timeInMilliseconds = function (timeString) {
        if (!timeString) { return 0; }

        let hour = parseFloat(timeString.substr(0, 2));
        let minute = parseFloat(timeString.substr(3, 2));

        return hour * 3600000 + minute * 60000;
    };

    var dateInMilliseconds = function (dateString) {
        if (!dateString) { return 0; }

        let date = new Date(dateString);
        if (!date) { return 0; }

        return date.getTime();
    };

    $min.stringToMilliseconds = function (timeString) {
        if (!timeString) { return 0; }

        let hour = parseFloat(timeString.substr(0, 2));
        let minute = parseFloat(timeString.substr(3, 2));

        return hour * 3600000 + minute * 60000;
    };

    $min.millisecondsToString = function (milliseconds) {
        if (!milliseconds) { return "00:00"; }

        let hours = Math.floor(milliseconds / 3600000);
        let minutes = Math.floor(milliseconds / 60000 % 60);
        //let seconds = Math.floor(value / 1000 % 60);

        return hours.toString().padStart(2, '0') + ":" + minutes.toString().padStart(2, '0');
    };

    // - Returns a cookie by name.
    $min.getCookie = function (name) {
        var cookie = decodeURIComponent(document.cookie);
        var values = cookie.split(";");
        for (var i = 0; i < values.length; i++) {
            var current = values[i];
            while (current.charAt(0) === " ") { current = current.substring(1); }
            if (current.indexOf(name) === 0) {
                return current.substring(name.length + 1, current.length);
            }
        }
    };

    // - Sets a cookie value.
    $min.setCookie = function (name, value) {
        document.cookie = name + "=" + value;
    };

    // - Removes a cookie by name.
    $min.delCookie = function (name) {
        document.cookie = name + "=; expires=Wed; 01 Jan 1970";
    };

    // - Returns the root URL.
    $min.root = function () {
        return window.location.origin ? window.location.origin + "/" : window.location.protocol + "//" + window.location.host + "/";
    };

    // - Returns the base URL.
    $min.base = function () {
        return new RegExp(/^.*\//).exec(window.location.href);
    };

    // - Sends an ajax request using [parameters] values as follows:
    // parameters: {
    //      url: "example.com",
    //      method: "GET|POST",
    //      headers: {
    //          "Content-Type": "application/json",
    //          "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
    //      },
    //      content: { },
    //      onsuccess: function(response) {
    //          // - Function on receiving a successful response.
    //      },
    //      onfailure: function(httpCode, httpMessage) {
    //          // - Function on failing to obtain a response.
    //      },
    // }
    $min.ajax = function (parameters) {
        let request = new XMLHttpRequest();
        let url = parameters.url;
        let method = parameters.method || "POST";
        let headers = parameters.headers || { "Content-Type": "application/json" };
        let content = parameters.content;
        let onsuccess = parameters.onsuccess;
        let onfailure = parameters.onfailure;
        //let requestType = parameters.requestType;

        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                if (request.status === 200) {
                    let responseType = request.getResponseHeader("Content-Type");
                    let response = null;

                    if (responseType.indexOf("application/json") > -1) {
                        response = JSON.parse(request.responseText);
                    }
                    else if (responseType.indexOf("text/xml") > -1) {
                        response = request.responseXML;
                    }
                    else { // responseType.indexOf("text/html")
                        response = request.responseText;
                    }

                    //let parser = new DOMParser();
                    //let doc = parser.parseFromString(response, "application/xml");

                    if (onsuccess) { onsuccess(response); }
                }
                else {
                    if (onfailure) { onfailure(request.status, request.statusText); }
                }
            }
        };

        request.open(method, url, true);
        for (let header in headers) {
            if (header.toUpperCase() === "CONTENT-TYPE" && (headers[header].toUpperCase() === "APPLICATION/X-WWW-FORM-URLENCODED" || headers[header].toUpperCase() === "MULTIPART/FORM-DATA")) {
                content = new FormData(content);
            }
            else {
                request.setRequestHeader(header, headers[header]);
            }

            if (header.toUpperCase() === "CONTENT-TYPE" && headers[header].toUpperCase() === "APPLICATION/JSON") {
                content = JSON.stringify(content);
            }
        }

        //if (requestType) {
        //    request.responseType = requestType;
        //}

        request.send(content);
    };

    // - Creates and returns a new element from [template] and appends it to the body element with a transparent background.
    $min.popup = function (template) {
        if (!template) { console.log("Template inválido."); return false; }

        let popup = document.importNode(template.content, true);
        let element = popup.firstElementChild;

        //document.body.append(popup);
        element.background = document.createElement("div");
        element.background.classList.add("popupground");
        element.background.onclick = function () {
            element.background.remove();
        };
        element.onclick = function (event) {
            (window.event || event).stopPropagation();
        };
        element.background.appendChild(popup);
        document.body.appendChild(element.background);
        //document.body.insertBefore(element.background, element);

        element.close = function () {
            element.background.remove();
            //element.remove();
        };

        return element;
    };

    // - Toggles the display (show/hide) of an existing popup element in the document.
    $min.togglePopup = function (element) {
        if (!element.background) {
            element.background = document.createElement("div");
            element.background.classList.add("popupground");
            element.background.onclick = function () {
                element.isVisible = false;
                element.background.style.display = "none";
            };
            element.onclick = function (event) {
                (window.event || event).stopPropagation();
            };
            element.parentNode.insertBefore(element.background, element);
            element.background.appendChild(element);
            element.close = function () {
                element.isVisible = false;
                element.background.style.display = "none";
            };
        }

        if (!element.isVisible) {
            element.isVisible = true;
            element.style.display = "block";
            element.background.style.display = "block";
        }
        else {
            element.isVisible = false;
            element.style.display = "none";
            element.background.style.display = "none";
        }
    };

    // - Simulates checkbox as radio elements within the element from [selector].
    $min.checkAsRadio = function (element, selector) {
        if (!element.radioGroup) {
            element.radioGroup = element.closest(selector);
        }

        let radios = element.radioGroup.querySelectorAll("[name='" + element.name + "']");
        for (let i = 0; i < radios.length; i++) {
            if (radios[i] === element) { continue; }
            radios[i].checked = false;
        }
    };

    // - Closes an element created by $min.popup(), if [selectorForPopup] is present it'll look for a closest match or it will use [element] instead.
    $min.close = function (element, selectorForPopup) {
        let popup = selectorForPopup ? element.closest(selectorForPopup) : element;

        popup.close();
    };

    // - Returns true if the event key pressed is numeric.
    $min.isNumeric = function (event) {
        event = event || window.event;
        switch (event.type) {
            case "keypress":
                {
                    let keyCode = event.which || event.keyCode;
                    if (keyCode === 13 || keyCode === 32) {
                        return true;
                    }

                    return !isNaN(String.fromCharCode(keyCode));
                }
            case "paste":
                {
                    let pasteData = event ? event.clipboardData ? event.clipboardData.getData('text/plain') : "" : window.event.clipboardData ? window.event.clipboardData.getData('text/plain') : "";

                    //if (event.preventDefault) {
                    //    event.stopPropagation();
                    //    event.preventDefault();
                    //}

                    return !isNaN(pasteData);
                }
        }
    };

    // - Returns true if the event key matches the [pattern] regex.
    $min.regex = function (event, pattern) {
        event = event || window.event;
        let keyCode = event.which || event.keyCode;

        if (keyCode === 13 || keyCode === 32) {
            return true;
        }

        return new RegExp(pattern).test(String.fromCharCode(keyCode));
    };

    // - Displays information about a image from a [input] file.
    // Ex:
    // <input type="file" onchange="$min.preview(this, callback)" />
    // <img preview="image" />
    // <span preview="name">The file name.</span>
    // <input type="hidden" preview="data" value="The file data." />
    $min.preview = function (input, callback) {
        if (!input.previews) {
            input.previews = {
                name: [],
                data: [],
                image: []
            };

            $min.forEach(input.parentNode, function (e) {
                console.log(e)
                if (e.hasAttribute("preview")) {
                    switch (e.getAttribute("preview")) {
                        
                        case "name":
                            { e.default = e.value || e.innerHTML; input.previews.name.push(e); break; }
                        case "data":
                            { input.previews.data.push(e); break; }
                        case "image":
                            { e.default = e.value || e.src || e.innerHTML; input.previews.image.push(e); break; }
                        default:
                            { break; }
                    }
                }
            });

            /*console.log(input.previews.data)*/

        }

        if (!input.value) {
            input.setAttribute("state", "empty");
            return;
        }

        let reader = new FileReader();
        reader.onload = function () {
            let dataURL = reader.result;
            let filename = input.value.indexOf('\\') > -1 ? input.value.substring(input.value.lastIndexOf('\\') + 1) : input.value;
            
            for (let i = 0; i < input.previews.name.length; i++) {
                let nodeName = input.previews.name[i].nodeName.toUpperCase();
                if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
                    input.previews.name[i].value = filename;
                }
                else {
                    input.previews.name[i].innerHTML = filename;
                }
            }
            
            for (let i = 0; i < input.previews.data.length; i++) {
                let nodeName = input.previews.data[i].nodeName.toUpperCase();
                if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
                    input.previews.data[i].value = dataURL.substring(dataURL.indexOf(',') + 1);
                }
                else {
                    input.previews.data[i].innerHTML = dataURL.substring(dataURL.indexOf(',') + 1);
                }
            }

            for (let i = 0; i < input.previews.image.length; i++) {
                input.previews.image[i].src = dataURL;
            }

            if (callback) { callback(input, dataURL); }
        };
        reader.readAsDataURL(input.files[0]);

        input.setAttribute("state", "loaded");
    };

    //$min.preview = function (input, callback) {
    //    if (!input.previews) {
    //        input.previews = {
    //            name: [],
    //            data: [],
    //            image: []
    //        };

    //        let card = input.closest('.card-prod-image').querySelectorAll('.image');
    //        console.log(card)
    //        for (let i = 0; i < card.length; i++) {
    //            $min.forEach(card[i].parentElement, function (e) {
    //                if (e.hasAttribute("preview")) {
    //                    switch (e.getAttribute("preview")) {

    //                        case "name":
    //                            { e.default = e.value || e.innerHTML; input.previews.name.push(e); break; }
    //                        case "data":
    //                            { input.previews.data.push(e); break; }
    //                        case "image":
    //                            { e.default = e.value || e.src || e.innerHTML; input.previews.image.push(e); break; }
    //                        default:
    //                            { break; }
    //                    }
    //                }
    //            });

    //            if (!input.value) {
    //                input.setAttribute("state", "empty");
    //                return;
    //            }

    //            let reader = new FileReader();
    //            reader.onload = function () {
    //                let dataURL = reader.result;
    //                let filename = input.value.indexOf('\\') > -1 ? input.value.substring(input.value.lastIndexOf('\\') + 1) : input.value;

    //                for (let i = 0; i < input.previews.name.length; i++) {
    //                    let nodeName = input.previews.name[i].nodeName.toUpperCase();
    //                    if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
    //                        input.previews.name[i].value = filename;
    //                    }
    //                    else {
    //                        input.previews.name[i].innerHTML = filename;
    //                    }
    //                }

    //                for (let i = 0; i < input.previews.data.length; i++) {
    //                    let nodeName = input.previews.data[i].nodeName.toUpperCase();
    //                    if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
    //                        input.previews.data[i].value = dataURL.substring(dataURL.indexOf(',') + 1);
    //                    }
    //                    else {
    //                        input.previews.data[i].innerHTML = dataURL.substring(dataURL.indexOf(',') + 1);
    //                    }
    //                }

    //                for (let i = 0; i < input.previews.image.length; i++) {
    //                    input.previews.image[i].src = dataURL;
    //                }

    //                if (callback) { callback(input, dataURL); }
    //            };
    //            reader.readAsDataURL(input.files[0]);

    //            input.setAttribute("state", "loaded");
    //        }
    //    }

        
    //};

    // - Prepares a [element] for a text scrambling effect.
    // Ex:
    // let effect = $min.scramble(document.getElementById("myObj"));
    // effect.start(10); // - 10 character scrambling
    // effect.stop("HelloWorld", " "); // - Ends the scrambling with "Hello World", and adds a " " as left padding.
    $min.scramble = function (element) {
        if (!element.scramble) {
            element.scramble = {};
            element.scramble.scrambles = ["/", "-", "+", " ", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
            element.scramble.target = "";
            element.scramble.skip = 0;
            element.scramble.size = element.innerHTML.length || 10;
            element.scramble.speed = 50;

            element.scramble.start = function (size) {
                if (element.scramble.interval) { return element.scramble; }

                let counter = 0;
                element.scramble.target = "";
                element.scramble.size = size || element.scramble.size;
                element.scramble.skip = 0;
                element.scramble.interval = setInterval(function () {
                    element.current = element.scramble.target.substr(0, element.scramble.skip);
                    for (let i = 0; i < element.scramble.size - element.scramble.skip; i++) {
                        element.current += element.scramble.scrambles[Math.floor(Math.random() * element.scramble.scrambles.length)];
                    }
                    element.innerHTML = element.current;

                    if (element.scramble.target) {
                        counter += 0.5;
                        element.scramble.skip = Math.floor(counter);
                    }
                    if (element.scramble.skip > element.scramble.target.length) {
                        clearInterval(element.scramble.interval);
                        element.scramble.interval = false;
                    }
                }, element.scramble.speed);

                return element.scramble;
            };

            element.scramble.stop = function (text, padding) {
                element.scramble.target = "";
                for (let i = 0; i < element.scramble.size - text.length; i++) {
                    element.scramble.target += padding || " ";
                }
                element.scramble.target += text;
                return element.scramble;
            };
        }
        return element.scramble;
    };

    // - Prepares a [element] for a text slide effect.
    // Ex: 
    // let effect = $min.slide(document.getElementById("myObj"));
    // effect.text(text, duration): queues the next text for the duration, in milliseconds, to appear.
    // effect.stop(): stops the effect (no more queues).
    $min.slide = function (element) {
        if (!element.slide) {
            element.slide = {};
            element.slide.texts = [];

            element.slide.opacity = 0;
            element.slide.timer = 0;
            element.slide.interval = setInterval(function () {
                if (element.slide.timer <= 0) {
                    element.slide.timer = 0;

                    if (element.slide.texts.length > 0) {
                        if (element.slide.opacity > 0) {
                            element.slide.opacity -= 0.025;
                            element.style.opacity = element.slide.opacity < 0 ? 0 : element.slide.opacity;
                        }
                        else {
                            element.slide.opacity = 0;
                            element.style.opacity = element.slide.opacity;

                            element.innerHTML = element.slide.texts[0].text;
                            element.slide.timer = element.slide.texts[0].duration;
                            element.slide.texts.remove(0);
                        }
                    }
                }
                else if (element.slide.opacity < 1) {
                    element.slide.opacity += 0.025;
                    element.style.opacity = element.slide.opacity > 1 ? 1 : element.slide.opacity;
                }
                element.slide.timer -= 10;
            }, 10);

            element.slide.stop = function () {
                clearInterval(element.slide.interval);
                element.slide.interval = false;
            };

            element.slide.text = function (text, milliseconds) {
                element.slide.texts.push({ "text": text, "duration": milliseconds });
                return element.slide;
            };
        }
        return element.slide;
    };

    $min.random = function (max, min) {
        return Math.floor(Math.random() * (max || 1)) + (min || 0);
    };
})(window.$min = window.$min || {});

(function ($ui) {
    // - Configuration
    $ui.redirectLoginURL = "Home/Login?ReturnURL=";
    $ui.relativePath = "";

    if (window.location.host.includes(".com.br")) {
        $ui.relativePath = "EstiloMB/";
    }

    $ui.checkEmpty = function (data) {
        if (!data) { return; }

        if (data.children.length === 0 || data.children.length === 1 && data.children[0].nodeName.toUpperCase() === "TEMPLATE") {
            data.setAttribute("state", "empty");
        }
        else {
            data.setAttribute("state", "default");
        }
    };

    // - Privates
    var emptyBusyMessage = "No busy message set.";

    var init = function (container) {
        let contents = container.querySelector("[ui-content='data']");
        if (!contents) { console.log("No element with @ui-content='data' found."); return false; }

        contents.isAutoLoad = (contents.getAttribute("ui-load") || "").toLowerCase() === "auto";

        if (!contents.initialized) {
            contents.container = container;
            contents.pixelAutoLoadThreshold = 25;
            contents.maxPageCount = parseInt(contents.getAttribute("maxPageCount"), 10) || 10;

            contents.url = contents.getAttribute("data-url");
            contents.template = contents.template || contents.querySelector(":scope > template") || false;
            //if (!contents.url) { console.log("No attribute @data-url found to load on the ui-content='data' contents."); return false; }            

            let elements = null;

            elements = container.querySelectorAll("[ui-content='filter']") || false;
            for (let i = 0; i < elements.length; i++) {
                let target = elements[i].hasAttribute("for") ? eval(elements[i].getAttribute("for").replace("this", "elements[i]")) : false;
                if (target) { target.filter = elements[i]; continue; }

                contents.filter = elements[i];
            }

            //elements = container.querySelectorAll("[ui-content='empty']") || false;
            //for (let i = 0; i < elements.length; i++) {
            //    let target = elements[i].hasAttribute("for") ? eval(elements[i].getAttribute("for").replace("this", "elements[i]")) : false;
            //    if (target) { target.empty = elements[i]; continue; }

            //    contents.empty = elements[i];
            //}

            //elements = container.querySelectorAll("[ui-content='loading']") || false;
            //for (let i = 0; i < elements.length; i++) {
            //    let target = elements[i].hasAttribute("for") ? eval(elements[i].getAttribute("for").replace("this", "elements[i]")) : false;
            //    if (target) { target.loading = elements[i]; continue; }

            //    contents.loading = elements[i];
            //}

            elements = container.querySelectorAll("[ui-content='paging']") || false;
            for (let i = 0; i < elements.length; i++) {
                let target = elements[i].hasAttribute("for") ? eval(elements[i].getAttribute("for").replace("this", "elements[i]")) : false;
                if (target) {
                    target.paging = elements[i];
                }
                else {
                    target = contents;
                    target.paging = elements[i];
                }

                target.paging.pages = target.paging.querySelector("[ui-content='pages']") || console.log("No element with the attribute @ui-content='pages' found in the children of the @ui-content='paging' contents.") || false;
                target.paging.prevPage = target.paging.querySelector("[ui-content='prev-page']") || console.log("No element with the attribute @ui-content='prev-page' found in the children of the @ui-content='paging' contents.") || false;
                target.paging.nextPage = target.paging.querySelector("[ui-content='next-page']") || console.log("No element with the attribute @ui-content='next-page' found in the children of the @ui-content='paging' contents.") || false;
            }

            contents.initialized = true;
        }

        return contents;
    };

    var load = function (contents, pageOffset) {

        if (contents.isBusy || !contents.url) {
            //$ui.message(contents.info.messageBusy || emptyBusyMessage, "alert"); 
            return;
        }
        //if (contents.loading) { contents.loading.classList.remove("hidden"); }
        //if (contents.empty) { contents.empty.classList.add("hidden"); }
        contents.isBusy = true;
        contents.setAttribute("state", "loading");

        if (pageOffset !== undefined) {
            contents.request.Page = contents.request.Page + pageOffset < 1 ? 1 : contents.request.Page + pageOffset;
        }

        if (contents.hasAttribute("onfilter")) {
            contents.request.Filter = eval(contents.getAttribute("onfilter").replace("this", "contents"));
        }

        $min.ajax({
            url: $min.root() + $ui.relativePath + contents.url,
            content: contents.request,
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                contents.isBusy = false;
                contents.setAttribute("state", "default");
                //if (contents.loading) { contents.loading.classList.add("hidden"); }

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                if (response.Total === 0) {
                    //if (contents.empty) { contents.empty.classList.remove("hidden"); }
                    //if (contents.paging) { contents.paging.classList.add("hidden"); }
                    contents.setAttribute("state", "empty");
                    return;
                }

                contents.request.Total = response.Total;
                contents.request.Count += response.Data.length;

                //let autoload = contents.loadType === "auto" || false;

                //if (contents.empty) { contents.empty.classList.add("hidden"); }

                // - No template, expecting a single contents.
                if (!contents.template) {
                    $min.bind(contents, response.Data);
                    return;
                }

                // - Has template, expecting a list.
                if (!contents.isAutoLoad) { $min.clear(contents); }

                for (let i = 0; i < response.Data.length; i++) {
                    let template = document.importNode(contents.template.content, true);
                    $min.bind(template, response.Data[i]);
                    template.firstElementChild.data = response.Data[i];
                    contents.appendChild(template);
                }

                // - No Paging
                if (contents.isAutoLoad === true) {
                    if (!contents.container.onscroll) {
                        contents.container.onscroll = function () {
                            if (contents.request.Count < contents.request.Total &&
                                contents.container.scrollHeight - contents.container.clientHeight - contents.container.scrollTop < contents.pixelAutoLoadThreshold) {
                                load(contents, 1);
                            }
                        };
                    }

                    if (contents.request.Count < contents.request.Total &&
                        contents.container.scrollHeight - contents.container.clientHeight - contents.container.scrollTop < contents.pixelAutoLoadThreshold) {
                        load(contents, 1);
                    }
                }
                // - Paging
                else if (contents.paging) {
                    contents.setAttribute("state", "paging");
                    //if (contents.request.Total > contents.request.PerPage) {
                    //    contents.paging.classList.remove("hidden");
                    //}
                    //else {
                    //    contents.paging.classList.add("hidden");
                    //}

                    // - Prev Button
                    if (contents.request.Page === 1) {
                        contents.paging.prevPage.disabled = true;
                        contents.paging.prevPage.classList.add("disabled");
                    }
                    else {
                        contents.paging.prevPage.disabled = false;
                        contents.paging.prevPage.classList.remove("disabled");
                    }

                    // - Next Button
                    if (contents.request.Page * contents.request.PerPage >= contents.request.Total) {
                        contents.paging.nextPage.disabled = true;
                        contents.paging.nextPage.classList.add("disabled");
                    }
                    else {
                        contents.paging.nextPage.disabled = false;
                        contents.paging.nextPage.classList.remove("disabled");
                    }

                    // - Pages
                    $min.clear(contents.paging.pages);
                    let pageCount = Math.ceil(contents.request.Total / contents.request.PerPage);
                    let pageStart = pageCount > contents.maxPageCount ? contents.request.Page - Math.floor(contents.maxPageCount / 2) : 1;
                    if (pageStart < 1) { pageStart = 1; }
                    let pageEnd = pageStart + contents.maxPageCount;
                    if (pageEnd > pageCount) { pageEnd = pageCount; }

                    if (pageStart > 1) {
                        let rets = document.createElement("span");
                        rets.innerHTML = "...";
                        contents.paging.pages.appendChild(rets);
                    }

                    for (let i = pageStart; i <= pageEnd; i++) {
                        let link = document.createElement("a");
                        link.innerHTML = i;

                        if (i === contents.request.Page) {
                            link.className = "selected";
                        }
                        else {
                            link.onclick = function () {
                                contents.request.Page = parseInt(this.innerHTML);
                                load(contents);
                            };
                        }

                        contents.paging.pages.appendChild(link);
                        contents.paging.pages.appendChild(document.createTextNode(" "));
                    }

                    if (pageEnd < pageCount) {
                        let rets = document.createElement("span");
                        rets.innerHTML = "...";
                        contents.paging.pages.appendChild(rets);
                    }
                }
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                    window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                }

                contents.isBusy = false;
                if (contents.loading) { contents.loading.classList.add("hidden"); }
                $ui.message(httpMessage, "error");
            }
        });
    };

    var isEmpty = function (contents) {
        /*if (!contents.empty) { return; }*/

        if (contents.children.length === 0 ||
            contents.children.length === 1 && contents.children[0].nodeName.toUpperCase() === "TEMPLATE") {
            //contents.empty.classList.remove("hidden");
            contents.setAttribute("state", "empty");
        }
        else {
            //contents.empty.classList.add("hidden");
            contents.setAttribute("state", "default");
        }
    };

    var getMessage = function (string, data) {
        let matches = (string || "").match(/\{[A-Za-z0-9\.]+(:.+)?\}/g);
        if (!matches) { return string; }

        for (let i = 0; i < matches.length; i++) {
            let object = data;
            let property = matches[i].replace(/[\{\}]/g, '');
            while (property.indexOf(".") > -1) {
                let member = property.substr(0, property.indexOf("."));
                object = object[member];
                property = property.substr(property.indexOf(".") + 1);
            }

            let value = null;
            if (property.indexOf(":") > 0) {
                value = $min.format(object[property.substr(0, property.indexOf(":"))], property.substr(property.indexOf(":") + 1));
            }
            else {
                value = object[property];
            }

            string = string.replace(matches[i], value);
        }

        return string;
    };

    $ui.isEmpty = function (container) {
        let contents = init(container);
        if (!contents) { return false; }

        isEmpty(contents);
    };

    // - Data Functions
    /* - Example:
    <table>
        <thead>
            <tr ui-content="filter">
                <td>
                </td>
            </tr>
        </thead>
        <tbody ui-content="data">
            <template>
                <tr>
                    <td>
                        DATA HERE
                    </td>
                </tr>
            </template>
        </tbody>
        <tfoot>
            <tr class="hidden" ui-content="loading">
                <td>
                    LOADING...
                </td>
            </tr>
            <tr class="hidden" ui-content="empty">
                <td>
                    NO ENTRIES FOUND.
                </td>
            </tr>
            <tr class="hidden" ui-content="paging">
                <td colspan="10">
                    <button ui-content="prev-page" onclick="$ui.prevPage(this, 'table')"></button>
                    <div ui-content="pages"></div>
                    <button ui-content="next-page" onclick="$ui.nextPage(this, 'table')"></button>
                </td>
            </tr>
        </tfoot>
    </table>
    */
    $ui.update = function (container) {
        let contents = init(container);
        if (!contents) { return false; }

        contents.request = {
            Page: parseFloat(contents.getAttribute("page")) || 1,
            PerPage: parseFloat(contents.getAttribute("per-page")) || (contents.template ? 10 : 1),
            Count: 0,
            Total: 0
        };

        if (contents.filter) {
            contents.request.Data = $min.read(contents.filter);
        }

        // - Has template, list expected, erasing current contents.
        if (contents.template) {
            $min.clear(contents);
        }
        console.log(contents);
        load(contents);
    };

    $ui.add = function (container, dataUrl, callback) {
        if (!container) {
            return console.log("ERROR in $ui.add(container, dataUrl, callback): The container parameter must be a valid element.");
        }

        let data = dataUrl ? container.querySelector("[ui-content=data][data-url='" + dataUrl + "']") : container.querySelector("[ui-content=data]");
        if (!data) {
            return console.log("ERROR in $ui.add(container, dataUrl, callback): No element with the attribute [ui-content=data] found within the container:", container);
        }

        data.template = data.template || data.querySelector(":scope > template") || false;
        if (!data.template) {
            return console.log("ERROR in $ui.add(container, dataUrl): A <template> element was not found in:", data);
        }

        let template = document.importNode(data.template.content, true);
        template.firstElementChild.isNew = true;
        let element = template.firstElementChild;

        if (data.getAttribute("insert-at") === "end") {
            data.appendChild(template);
        }
        else {
            data.insertBefore(template, data.firstElementChild || null);
        }

        $ui.checkEmpty(data);

        if (callback) {
            callback(element);
        }

        return element;
    };

    $ui.edit = function (button, itemSelector) {
        let item = itemSelector ? button.closest(itemSelector) : button;

        $min.forEach(item, function (e) {
            let nodeName = e.nodeName.toUpperCase();
            if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
                e.disabled = false;
            }
        });

        $min.toggle(item, "edit");
        item.isEditing = true;
    };

    $ui.cancel = function (button, itemSelector) {
        let item = itemSelector ? button.closest(itemSelector) : button;

        if (item.isPopup) {
            item.background.remove();
            return;
        }

        if (item.isNew) {
            item.remove();
            return;
        }

        $min.forEach(item, function (e) {
            let nodeName = e.nodeName.toUpperCase();
            if (nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA") {
                e.disabled = true;
            }
        });

        if (item.data) {
            $min.bind(item, item.data);
        }

        $min.toggle(item, "view");
        item.isEditing = false;
    };

    $ui.save = function (button, itemSelector, callback) {
        let url = button.getAttribute("data-url") || false;
        if (!url) { console.log("No @data-url attribute found on the element calling $ui.save(this)."); return false; }
        let item = button.closest(itemSelector);
        if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

        if (!$min.validate(item, true)) { return; }
        let data = $min.read(item);

        let contents = item.isPopup ? item.contents : item.closest("[ui-content=data]");
        if (contents && contents.filter) {
            $min.read(contents.filter, data);
        }

        button.disabled = true;
        item.isBusy = true;

        $min.ajax({
            url: $min.root() + $ui.relativePath + url,
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                button.disabled = false;
                item.isBusy = false;

                if (response.Code !== 0) {
                    $min.bind(item, response.Data, "error");
                    return $ui.message(response.Message, "error");
                }

                if (item.isPopup) {
                    if (item.isNew) {
                        let newItem = document.importNode(contents.template.content, true);
                        item.original = newItem.firstElementChild;
                        contents.appendChild(newItem);
                    }

                    item.original.data = response.Data;
                    $min.bind(item.original, response.Data);
                }

                let contents = item.closest('.page').querySelectorAll('tr.opacity-animation');
                

                //for (let i = 0; i < contents.length; i++) {
                //    console.log(response.Data)
                //    if (contents[i].data.ID == response.Data.ID) {
                //        console.log(contents[i])
                //        $min.bind(contents[i], response.Data);
                //    }
                //}

                /*$min.bind(item.closest('.item'), response.Data);*/
                item.data = response.Data;
                item.isNew = false;

                $ui.message(getMessage(button.getAttribute("ui-saved-message"), data), "success");
                $ui.update(item.closest('.item'));

                if (callback) {
                    callback(button, itemSelector, response);
                }
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                    window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                }

                button.disabled = false;
                item.isBusy = false;
                $ui.message(httpMessage, "error");
            }
        });
    };

    $ui.recusado = function (button, itemSelector, callback) {

        let url = button.getAttribute("data-url") || false;
        if (!url) { console.log("No @data-url attribute found on the element calling $ui.save(this)."); return false; }

        let item = button.closest(itemSelector);
        if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

        if (!$min.validate(item, true)) { return; }
        let data = $min.read(item);

        let contents = item.isPopup ? item.contents : item.closest("[ui-content=data]");
        if (contents && contents.filter) {
            $min.read(contents.filter, data);
        }

        button.disabled = true;
        item.isBusy = true;

        $min.ajax({
            url: $min.root() + $ui.relativePath + url,
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                button.disabled = false;
                item.isBusy = false;

                if (response.Code !== 0) {
                    $min.bind(item, response.Data, "error");
                    return $ui.message(response.Message, "error");
                }

                if (item.isPopup) {
                    if (item.isNew) {
                        let newItem = document.importNode(contents.template.content, true);
                        item.original = newItem.firstElementChild;
                        contents.appendChild(newItem);
                    }

                    item.original.data = response.Data;
                    $min.bind(item.original, response.Data);
                }

                $min.bind(item, response.Data);
                item.data = response.Data;
                item.isNew = false;

                $ui.message(getMessage(button.getAttribute("ui-saved-message"), data), "success");

                if (callback) {
                    callback(button, itemSelector, response);
                }
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                    window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                }

                button.disabled = false;
                item.isBusy = false;
                $ui.message(httpMessage, "error");
            }
        });
    };

    $ui.inactive = function (button, itemSelector, callback) {
        let url = button.getAttribute("data-url") || false;
        if (!url) { console.log("No @data-url attribute found on the element calling $ui.delete(this)."); return false; }

        let item = typeof itemSelector === "string" ? button.closest(itemSelector) : itemSelector;
        let contents = item.isPopup ? item.contents : item.closest("[ui-content=data]");

        if (item.isNew) {
            if (item.isPopup) {
                item.background.remove();
            }


            isEmpty(contents);

            if (callback) {
                callback(button, itemSelector);
            }
            return;
        }

        if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

        let data = item.data;

        let popup = $min.popup(document.getElementById("ui-popup"));
        if (!popup) { return false; }

        let text = popup.querySelector("[ui-content=text]");

        text.innerHTML = getMessage(button.getAttribute("ui-confirm-message"), data);

        let confirm = popup.querySelector("[ui-action=confirm]");
        confirm.onclick = function () {
            button.disabled = true;
            item.isBusy = true;

            $min.ajax({
                url: $min.root() + $ui.relativePath + url,
                content: { Data: data },
                headers: {
                    "Content-Type": "application/json",
                    "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                },
                onsuccess: function (response) {
                    button.disabled = false;
                    item.isBusy = false;

                    if (response.Code !== 0) {
                        $min.bind(item, response.Data, "error");
                        popup.close();
                        return $ui.message(response.Message, "error");
                    }

                    if (item.isPopup) {
                        if (item.isNew) {
                            let newItem = document.importNode(contents.template.content, true);
                            item.original = newItem.firstElementChild;
                            contents.appendChild(newItem);
                        }

                        item.original.data = response.Data;
                        $min.bind(item.original, response.Data);
                    }

                    $min.bind(item, response.Data);
                    item.data = response.Data;
                    item.isNew = false;

                    isEmpty(contents);

                    popup.close();
                    $ui.message(getMessage(button.getAttribute("ui-deleted-message"), data), "success");


                    if (callback) {
                        callback(button, itemSelector, response);
                    }
                },
                onfailure: function (httpCode, httpMessage) {
                    if (httpCode === 401) {
                        let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                        window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                    }

                    button.disabled = false;
                    item.isBusy = false;
                    $ui.message(httpMessage, "error");
                }
            });
        };

        let cancel = popup.querySelector("[ui-action=cancel]");
        cancel.onclick = function () {
            popup.close();
        };
    };

    $ui.delete = function (button, itemSelector, callback) {
        
        let url = button.getAttribute("data-url") || false;
        console.log(itemSelector);
        if (!url) { console.log("No @data-url attribute found on the element calling $ui.delete(this)."); return false; }
        
        let item = typeof itemSelector === "string" ? button.closest(itemSelector) : itemSelector;
        let contents = item.isPopup ? item.contents : item.closest("[ui-content=data]");
        
        if (item.isNew) {
            if (item.isPopup) {
                item.background.remove();
            }
            else {
                item.remove();
            }

            isEmpty(contents);

            if (callback) {
                callback(button, itemSelector);
            }
            return;
        }

        if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

        let data = item.data;

        let popup = $min.popup(document.getElementById("ui-popup"));
        if (!popup) { return false; }

        let text = popup.querySelector("[ui-content=text]");

        text.innerHTML = getMessage(button.getAttribute("ui-confirm-message"), data);

        let confirm = popup.querySelector("[ui-action=confirm]");
        confirm.onclick = function () {
            button.disabled = true;
            item.isBusy = true;

            $min.ajax({
                url: $min.root() + $ui.relativePath + url,
                content: { Data: data },
                headers: {
                    "Content-Type": "application/json",
                    "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                },
                onsuccess: function (response) {
                    button.disabled = false;
                    item.isBusy = false;

                    if (response.Code !== 0) {
                        $min.bind(item, response.Data, "error");
                        popup.close();
                        return $ui.message(response.Message, "error");
                    }

                    if (item.isPopup) {
                        item.background.remove();
                        item.original.remove();
                    }
                    else {
                        item.remove();
                    }

                    isEmpty(contents);

                    popup.close();
                    $ui.message(getMessage(button.getAttribute("ui-deleted-message"), data), "success");


                    if (callback) {
                        callback(button, itemSelector, response);
                    }
                },
                onfailure: function (httpCode, httpMessage) {
                    if (httpCode === 401) {
                        let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                        window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                    }

                    button.disabled = false;
                    item.isBusy = false;
                    $ui.message(httpMessage, "error");
                }
            });
        };

        let cancel = popup.querySelector("[ui-action=cancel]");
        cancel.onclick = function () {
            popup.close();
        };
    };

    $ui.deletePrazo = function (button, itemSelector, callback) {
        let url = button.getAttribute("data-url") || false;
        if (!url) { console.log("No @data-url attribute found on the element calling $ui.delete(this)."); return false; }

        let item = typeof itemSelector === "string" ? button.closest(itemSelector) : itemSelector;
        let contents = item.isPopup ? item.contents : item.closest("[ui-content=data]");

        if (item.parentNode.querySelector('[name = Status]').value == '1') {

            if (item.isNew) {
                if (item.isPopup) {
                    item.background.remove();
                }
                else {
                    item.remove();
                }

                isEmpty(contents);

                if (callback) {
                    callback(button, itemSelector);
                }
                return;
            }

            if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

            let data = item.data;

            let popup = $min.popup(document.getElementById("ui-popup"));
            if (!popup) { return false; }

            let text = popup.querySelector("[ui-content=text]");

            text.innerHTML = getMessage(button.getAttribute("ui-confirm-message"), data);

            let confirm = popup.querySelector("[ui-action=confirm]");
            confirm.onclick = function () {
                button.disabled = true;
                item.isBusy = true;

                $min.ajax({
                    url: $min.root() + $ui.relativePath + url,
                    content: { Data: data },
                    headers: {
                        "Content-Type": "application/json",
                        "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                    },
                    onsuccess: function (response) {
                        button.disabled = false;
                        item.isBusy = false;

                        if (response.Code !== 0) {
                            $min.bind(item, response.Data, "error");
                            popup.close();
                            return $ui.message(response.Message, "error");
                        }

                        if (item.isPopup) {
                            item.background.remove();
                            item.original.remove();
                        }
                        else {
                            item.remove();
                        }

                        isEmpty(contents);

                        $ui.message(getMessage(button.getAttribute("ui-deleted-message"), data), "success");
                        popup.close();

                        if (callback) {
                            callback(button, itemSelector, response);
                        }
                    },
                    onfailure: function (httpCode, httpMessage) {
                        if (httpCode === 401) {
                            let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                            window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                        }

                        button.disabled = false;
                        item.isBusy = false;
                        $ui.message(httpMessage, "error");
                    }
                });
            };

            let cancel = popup.querySelector("[ui-action=cancel]");
            cancel.onclick = function () {
                popup.close();
            };

        } else {

            if (item.isNew) {
                if (item.isPopup) {
                    item.background.remove();
                }
                else {
                    item.remove();
                }

                isEmpty(contents);

                if (callback) {
                    callback(button, itemSelector);
                }
                return;
            }

            if (item.isBusy) { return $ui.message(button.getAttribute("ui-busy-message") || emptyBusyMessage, "alert"); }

            let data = item.data;

            let popup = $min.popup(document.getElementById("ui-popup"));
            let text = popup.querySelector("[ui-content=text]");

            text.innerHTML = getMessage(button.getAttribute("ui-confirm-message"), data);

            let confirm = popup.querySelector("[ui-action=confirm]");
            confirm.onclick = function () {

                button.disabled = true;
                item.isBusy = true;

                $min.ajax({
                    url: $min.root() + $ui.relativePath + url,
                    content: { Data: data },
                    headers: {
                        "Content-Type": "application/json",
                        "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                    },
                    onsuccess: function (response) {
                        button.disabled = false;
                        item.isBusy = false;

                        if (response.Code !== 0) {
                            $min.bind(item, response.Data, "error");
                            popup.close();
                            return $ui.message(response.Message, "error");
                        }

                        if (item.isPopup) {
                            item.background.remove();
                            item.original.remove();
                        }
                        else {
                            item.remove();
                        }

                        isEmpty(contents);

                        popup.close();
                        $ui.message(getMessage(button.getAttribute("ui-deleted-message"), data), "success");


                        if (callback) {
                            callback(button, itemSelector, response);
                        }

                    },
                    onfailure: function (httpCode, httpMessage) {
                        if (httpCode === 401) {
                            let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                            window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                        }

                        button.disabled = false;
                        item.isBusy = false;
                        $ui.message(httpMessage, "error");
                    }
                });
            };

            let cancel = popup.querySelector("[ui-action=cancel]");
            cancel.onclick = function () {
                popup.close();
            };

        }
    };

    //$ui.remove = function (button, itemSelector) {
    //    let item = button.closest(itemSelector);
    //    let contents = item.closest("[ui-content=data]");

    //    item.remove();

    //    isEmpty(contents);
    //};

    $ui.remove = function (item) {
        if (!item) {
            return console.log("ERROR in $ui.remove(item): The item parameter must be a valid element.");
        }

        let data = item.closest("[ui-content=data]");

        item.remove();

        $ui.checkEmpty(data);
    };

    $ui.restore = function (button, itemSelector) {
        let item = button.closest(itemSelector);

        if (item.data) {
            $min.bind(item, item.data);
        }
    };

    $ui.nextPage = function (button, containerSelector) {
        let container = button.closest(containerSelector);
        let contents = init(container);

        load(contents, 1);
    };

    $ui.prevPage = function (button, containerSelector) {
        let container = button.closest(containerSelector);
        let contents = init(container);

        load(contents, -1);
    };

    $ui.popup = function (container, item, template, dataUrl) {
        if (!container) {
            return console.log("ERROR in $ui.popup(container, item, template, dataUrl): The container parameter must be a valid element.");
        }

        if (!template) {
            return console.log("ERROR in $ui.popup(container, item, template, dataUrl): The template parameter must be a valid element.");
        }

        let data = container.querySelector("[ui-content=data][data-url='" + dataUrl + "']") || container.querySelector("[ui-content=data]") || false;
        let popup = $min.popup(template);
        popup.isChildData = true;
        popup.parentData = data;

        if (item) {
            popup.original = item;
            popup.data = item.data;
            $min.bind(popup, item.data);
        }
        else {
            popup.isNew = true;
        }
        return popup;
    };

    $ui.filter = function (element, containerSelector, propertyName) {
        let data = element.closest(containerSelector).querySelectorAll("[name='" + propertyName + "']");
        let filter = [];

        for (let i = 0; i < data.length; i++) {
            let item = {};
            item[propertyName] = data[i].value;

            filter.push(item);
        }

        return filter;
    };

    $ui.sort = function (element, childSelector, compareFunction) {
        let children = element.querySelectorAll(childSelector);
        let array = Array.from(children);

        array.sort(compareFunction);

        for (let i = 0; i < array.length; i++) {
            element.appendChild(array[i]);
        }
    };

    $ui.addFile = function (input, targetContainer) {
        let template = targetContainer.querySelector(":scope > template");
        if (template) {
            targetContainer.template = template;
            template.remove();
        }

        if (!input.value) { return; }

        let reader = new FileReader();
        reader.onload = function () {
            let template = document.importNode(targetContainer.template.content, true);
            $min.bind(template, {
                Caminho: input.value,
                Formato: reader.result.substring(0, reader.result.indexOf(',') + 1),
                Arquivo: input.value.indexOf('\\') > -1 ? input.value.substring(input.value.lastIndexOf('\\') + 1) : input.value,
                ArquivoData: reader.result.substring(reader.result.indexOf(',') + 1)
            });

            targetContainer.appendChild(template);
            input.value = "";
        };
        reader.readAsDataURL(input.files[0]);
    };

    var selects = [];
    $ui.updateSelect = function (select, targetSelect, callback) {
        //let targetSelect = $min.findAfter(select, function (e) { return e.name === childSelectName; });
        if (!targetSelect) {
            return console.log("No target select specified.");
        }
        if (!targetSelect.hasAttribute("data-url")) {
            return console.log("No @data-url attribute found on the target <select name='" + targetSelect.name + "'>.");
        }
        if (!targetSelect.hasAttribute("data-property")) {
            return console.log("No @data-property attribute found on the target <select name='" + targetSelect.name + "'>.");
        }

        if (!targetSelect.defaultOptions) {
            targetSelect.defaultOptions = [];
            $min.forEach(targetSelect, function (element) {
                targetSelect.defaultOptions.push(element);
            });
        }

        if (select.value === "" && !select.queuedValue) {
            $min.clear(targetSelect);

            if (targetSelect.defaultOptions) {
                for (let j = 0; j < targetSelect.defaultOptions.length; j++) {
                    targetSelect.appendChild(targetSelect.defaultOptions[j].cloneNode(true));
                }
            }

            return;
        }
        // - Já carregado
        if (selects[select.name]) {
            selects[select.name].load(select.queuedValue || select.value, targetSelect, callback);
            return;
        }

        //console.log(select);

        // - Criando a lista
        selects[select.name] = {
            values: [],
            load: function (value, childSelect, callBack) {
                if (!selects[select.name].values[value]) {
                    selects[select.name].values[value] = {};
                }

                if (!selects[select.name].values[value][childSelect.name]) {
                    selects[select.name].values[value][childSelect.name] = {
                        status: 0,
                        options: null,
                        queuedSelects: [],
                        queuedCallbacks: []
                    };
                }

                let currentList = selects[select.name];
                let currentValue = selects[select.name].values[value][childSelect.name];

                // - Clearing child select regardless of loading status
                if (!childSelect.defaultOptions) {
                    childSelect.defaultOptions = [];
                    $min.forEach(childSelect, function (element) {
                        childSelect.defaultOptions.push(element);
                    });
                }

                $min.clear(childSelect);

                switch (currentValue.status) {
                    case 0:
                        {
                            //console.log("Loading " + select.name + ": " + value + ", " + childSelect.name);
                            //if (childSelect) {
                            currentValue.queuedSelects.push(childSelect);
                            //}

                            if (callBack) {
                                currentValue.queuedCallbacks.push(callBack);
                            }

                            let data = { Data: {} };
                            data.Data[select.name] = value;
                            currentValue.status = 1;

                            $min.ajax({
                                url: $min.root() + $ui.relativePath + childSelect.getAttribute("data-url"),
                                content: data,
                                headers: {
                                    "Content-Type": "application/json",
                                    "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                                },
                                onsuccess: function (response) {
                                    if (response.Code !== 0) {
                                        return $ui.message(response.Message, "error");
                                    }

                                    //console.log("Finished Loading " + select.name + ": " + value);

                                    currentValue.options = response.Data;
                                    currentValue.status = 2;
                                    currentList.load(value, childSelect);
                                },
                                onfailure: function (httpCode, httpMessage) {
                                    $ui.message(httpCode + ": " + httpMessage, "error");
                                }
                            });
                            break;
                        }
                    case 1:
                        {
                            //console.log("Currently Loading " + select.name + ": " + value + ", queueing.");

                            if (childSelect && !currentValue.queuedSelects.includes(childSelect)) {
                                currentValue.queuedSelects.push(childSelect);
                            }

                            if (callBack) {
                                currentValue.queuedCallbacks.push(callBack);
                            }
                            break;
                        }
                    case 2:
                        {
                            //console.log("Loaded " + select.name + ": " + value + ", " + childSelect.name + ", filling values.");
                            if (childSelect && !currentValue.queuedSelects.includes(childSelect)) {
                                currentValue.queuedSelects.push(childSelect);
                            }

                            if (callBack) {
                                currentValue.queuedCallbacks.push(callBack);
                            }

                            for (let i = currentValue.queuedSelects.length - 1; i >= 0; i--) {
                                let currentSelect = currentValue.queuedSelects[i];
                                let selectValue = currentValue.queuedSelects[i].value || currentValue.queuedSelects[i].queuedValue;
                                delete currentValue.queuedSelects[i].queuedValue;

                                if (currentSelect.defaultOptions) {
                                    for (let j = 0; j < currentSelect.defaultOptions.length; j++) {
                                        currentSelect.appendChild(currentSelect.defaultOptions[j].cloneNode(true));
                                    }
                                }

                                let idProperty = currentSelect.getAttribute("data-bind") || currentSelect.getAttribute("name");
                                let dataProperty = currentValue.queuedSelects[i].getAttribute("data-property");

                                for (let j = 0; j < currentValue.options.length; j++) {
                                    let option = document.createElement("option");
                                    option.value = currentValue.options[j][idProperty];
                                    option.innerHTML = currentValue.options[j][dataProperty] || "-";
                                    if (selectValue === currentValue.options[j][idProperty]) {
                                        option.selected = true;
                                    }
                                    currentSelect.appendChild(option);
                                }

                                currentValue.queuedSelects.remove(i);
                            }

                            for (let i = currentValue.queuedCallbacks.length - 1; i >= 0; i--) {
                                //currentValue.queuedCallbacks[i](currentList.element, childSelect);
                                currentValue.queuedCallbacks[i]();
                                currentValue.queuedCallbacks.remove(i);
                            }
                            break;
                        }
                }

                return currentValue;
            }
        };

        selects[select.name].load(select.queuedValue || select.value, targetSelect, callback);
        return;
    };

    $ui.bindQueuedValue = function (select, value) {
        select.queuedValue = value;
    };

    // - Pra usar no onfocus
    $ui.loadSelect = function (select, containerSelector) {
        let container = select.closest(containerSelector);

        if (!container.precisaUpdate) {
            return;
        }

        // - faz tudo aqui
        container.precisaUpdate = false;
    };

    // - Form Functions
    $ui.inputCPFouCNPJ = function (input) {
        if (input.value.length <= 14) {
            $min.pattern(input, "000.000.000-00");
        }
        else {
            $min.pattern(input, "00.000.000/0000-00");
        }
    };

    $ui.inputCNPJ = function (input) {
        $min.pattern(input, "00.000.000/0000-00");
    };

    $ui.inputRG = function (input) {
        if (input.value.length <= 12) {
            $min.pattern(input, "00.000.000-0");
        }
        else if (input.value.length === 13) {
            $min.pattern(input, "000.000.000-0");
        }
        else if (input.value.length === 14) {
            $min.pattern(input, "0.000.000.000-0");
        }
        else if (input.value.length === 15) {
            $min.pattern(input, "00.000.000.000-0");
        }
        else if (input.value.length === 16) {
            $min.pattern(input, "000.000.000.000-0");
        }
    };

    $ui.inputTelefone = function (input) {
        if (input.value.length <= 14) {
            $min.pattern(input, "(00) 0000-0000");
        }
        else {
            $min.pattern(input, "(00) 00000-0000");
        }
    };

    $ui.inputCEP = function (input) {
        $min.pattern(input, "00000-000");
    };

    // - Validation Functions
    $ui.validateCPF = function (input) {
        let cpf = input.value.replace(/[^\d]+/g, '');
        if (cpf == '') { return true; }

        // Elimina CPFs invalidos conhecidos
        if (cpf.length != 11 ||
            cpf == "00000000000" ||
            cpf == "11111111111" ||
            cpf == "22222222222" ||
            cpf == "33333333333" ||
            cpf == "44444444444" ||
            cpf == "55555555555" ||
            cpf == "66666666666" ||
            cpf == "77777777777" ||
            cpf == "88888888888" ||
            cpf == "99999999999") {
            return false;
        }

        // Valida 1o digito	
        let add = 0;
        for (let i = 0; i < 9; i++) {
            add += parseInt(cpf.charAt(i)) * (10 - i);
        }
        let rev = 11 - (add % 11);
        if (rev == 10 || rev == 11) {
            rev = 0;
        }
        if (rev != parseInt(cpf.charAt(9))) {
            return false;
        }

        // Valida 2o digito	
        add = 0;
        for (let i = 0; i < 10; i++) {
            add += parseInt(cpf.charAt(i)) * (11 - i);
        }
        rev = 11 - (add % 11);
        if (rev == 10 || rev == 11) {
            rev = 0;
        }
        if (rev != parseInt(cpf.charAt(10))) {
            return false;
        }

        return true;
    };

    $ui.validateCNPJ = function (input) {
        let cnpj = input.value.replace(/[^\d]+/g, '');
        if (cnpj == '') { return true; }

        // Elimina CNPJs invalidos conhecidos
        if (cnpj.length != 14 ||
            cnpj == "00000000000000" ||
            cnpj == "11111111111111" ||
            cnpj == "22222222222222" ||
            cnpj == "33333333333333" ||
            cnpj == "44444444444444" ||
            cnpj == "55555555555555" ||
            cnpj == "66666666666666" ||
            cnpj == "77777777777777" ||
            cnpj == "88888888888888" ||
            cnpj == "99999999999999") {
            return false;
        }

        // Valida DVs
        let tamanho = cnpj.length - 2;
        let numeros = cnpj.substring(0, tamanho);
        let digitos = cnpj.substring(tamanho);
        let soma = 0;
        let pos = tamanho - 7;
        for (let i = tamanho; i >= 1; i--) {
            soma += numeros.charAt(tamanho - i) * pos--;
            if (pos < 2) {
                pos = 9;
            }
        }
        let resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
        if (resultado != digitos.charAt(0)) {
            return false;
        }

        tamanho = tamanho + 1;
        numeros = cnpj.substring(0, tamanho);
        soma = 0;
        pos = tamanho - 7;
        for (let i = tamanho; i >= 1; i--) {
            soma += numeros.charAt(tamanho - i) * pos--;
            if (pos < 2) {
                pos = 9;
            }
        }
        resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
        if (resultado != digitos.charAt(1)) {
            return false;
        }

        return true;
    };

    $ui.validateDate = function (input) {
        if (input.value) {
            if (new Date(input.value) > new Date()) {
                return false;
            }
        }
        return true;
    };

    $ui.validateEmail = function (input) {
        let str = input.value;
        let regex = /[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?/;
        if (regex.test(str)) {
            return true;
        }
        else {
            return false;
        }
    };

    // - Form Functions
    $ui.setFormPrompt = function (form, message) {
        $min.forEach(form, function (e) {
            let nodeName = e.nodeName.toUpperCase();
            if (nodeName !== "INPUT" && nodeName !== "SELECT" && nodeName !== "TEXTAREA") { return; }

            e.addEventListener("change", function () {
                form.hasChanges = true;
            });
        });

        window.addEventListener("beforeunload", function (e) {
            if (!form.hasChanges) {
                return undefined;
            }

            let confirmationMessage = message || "Are you sure you want to leave this page? Changed will be lost.";

            (e || window.event).returnValue = confirmationMessage; //Gecko + IE
            return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
        });
    };

    $ui.clearFormPrompt = function (form) {
        form.hasChanges = false;
    };

    $ui.bindError = function (validationElement) {
        if (!validationElement.input) { validationElement.input = $min.find(validationElement.parentNode, function (e) { let nodeName = e.nodeName.toUpperCase(); return nodeName === "INPUT" || nodeName === "SELECT" || nodeName === "TEXTAREA"; }); }

        if (validationElement.innerHTML) {
            validationElement.input.classList.remove("valid");
            validationElement.input.classList.add("error");
        }
        else {
            validationElement.input.classList.remove("error");
            validationElement.input.classList.add("valid");
        }
    };

    // - Notifications
    /* - Example:
    <div id="ui-messages">
        <template>
            <div class="ui-message" onclick="this.className += ' closing'">
                <i class="fas fa-check-circle icon-success"></i>
                <i class="fas fa-times-circle icon-error"></i>
                <i class="fas fa-exclamation-triangle icon-alert"></i>

                <div class="ui-text" ui-content="text"></div>
            </div>
        </template>
    </div>
    */
    var messageSettings = {
        elementID: "ui-messages",
        container: null,
        template: null,
        displayTimeoutMS: 5000,
        animationTimeoutMS: 1000,
    };

    $ui.message = function (content, className) {
        if (content === undefined || content === null) {
            return;
        }

        if (!messageSettings.container) {
            messageSettings.container = document.getElementById(messageSettings.elementID);
            messageSettings.template = messageSettings.container.firstElementChild;
        }

        let template = document.importNode(messageSettings.template.content, true);
        let message = template.firstElementChild;
        let text = message.querySelector("[ui-content=text]");
        if (text) {
            if (typeof content === "string") {
                text.innerHTML = content;
            }
            else if (content.nodeType === 1) {
                text.appendChild(content);
            }
            else {
                text.innerHTML = "Unimplemented data type for the notifications content.";
                console.log(content);
            }
        }

        message.classList.add(className);
        message.timer = setTimeout(function () {
            message.classList.add("closing");
            message.removeTimer = setTimeout(function () {
                message.remove();
            }, messageSettings.animationTimeoutMS);
        }, messageSettings.displayTimeoutMS);

        messageSettings.container.appendChild(template);
    };

    // - Calendar Functions
    var calendarSettings = {
        elementID: "ui-calendar"
    };

    var updateCalendar = function (calendar) {
        let templateWeek = calendar.querySelectorAll("[ui-template=week]");
        for (let i = 0; i < templateWeek.length; i++) {
            templateWeek[i].remove();
        }

        let templateDay = calendar.querySelectorAll("[ui-template=day]");
        for (let i = 0; i < templateDay.length; i++) {
            templateDay[i].remove();
        }

        // - Date
        let date = new Date();
        if (calendar.year !== undefined) { date.setFullYear(calendar.year); } else { calendar.year = date.getFullYear(); }
        if (calendar.month !== undefined) { date.setMonth(calendar.month); } else { calendar.month = date.getMonth(); }

        let year = date.getFullYear();
        let month = date.getMonth();

        let regex = null;
        for (let i = 0; i < calendar.evalElements.length; i++) {
            let expression = calendar.evalElements[i].expression;
            while (regex = expression.match(/{{(.+)}}/)) {
                expression = expression.replace(regex[0], eval(regex[1]));
            }

            calendar.evalElements[i].innerHTML = expression;
        }

        // - Week/Days
        let today = new Date();
        today.setHours(0, 0, 0, 0);

        let fromDay = new Date(year, month);
        let toDay = new Date(fromDay);

        fromDay.setDate(fromDay.getDate() - fromDay.getDay());
        toDay.setMonth(toDay.getMonth() + 1);
        toDay.setDate(toDay.getDate() - 1);
        toDay.setDate(toDay.getDate() + 7 - toDay.getDay());

        let weekday = 7;
        let week = null;

        while (fromDay < toDay) {
            if (weekday % 7 === 0) {
                weekday = 1;
                week = calendar.templateWeek.cloneNode(true);
                calendar.templateWeek.target.appendChild(week);
            }
            else {
                weekday++;
            }

            let day = null;
            switch (weekday) {
                case 1: { day = calendar.templateDay.cloneNode(true); day.classList.add("sunday"); week.appendChild(day); break; }
                case 2: { day = calendar.templateDay.cloneNode(true); day.classList.add("monday"); week.appendChild(day); break; }
                case 3: { day = calendar.templateDay.cloneNode(true); day.classList.add("thursday"); week.appendChild(day); break; }
                case 4: { day = calendar.templateDay.cloneNode(true); day.classList.add("wednesday"); week.appendChild(day); break; }
                case 5: { day = calendar.templateDay.cloneNode(true); day.classList.add("tuesday"); week.appendChild(day); break; }
                case 6: { day = calendar.templateDay.cloneNode(true); day.classList.add("friday"); week.appendChild(day); break; }
                case 7: { day = calendar.templateDay.cloneNode(true); day.classList.add("saturday"); week.appendChild(day); break; }
            }

            day.setAttribute("date", fromDay.getFullYear() + "-" + (fromDay.getMonth() + 1).toString().padStart(2, "0") + "-" + fromDay.getDate().toString().padStart(2, "0"));
            day.innerHTML = day.innerHTML.replace("{{day}}", fromDay.getDate());

            if (fromDay.getFullYear() === today.getFullYear() && fromDay.getMonth() === today.getMonth() && fromDay.getDate() === today.getDate()) {
                day.classList.add("today");
            }
            else if (fromDay.getMonth() < month) {
                day.classList.add("prev-month");
            }
            else if (fromDay.getMonth() > month) {
                day.classList.add("next-month");
            }

            //if (element.selected &&
            //    element.selected.getFullYear() === fromDay.getFullYear() &&
            //    element.selected.getMonth() === fromDay.getMonth() &&
            //    element.selected.getDate() === fromDay.getDate()) {
            //    day.classList.add("selected");
            //}

            fromDay.setDate(fromDay.getDate() + 1);
        }
    };

    var checkCalendarFocus = function (element) {
        clearTimeout(element.closeTimeout);
        element.closeTimeout = setTimeout(function () {
            if (!element.calendar) { return; }
            if (document.activeElement !== element) {
                element.calendar.remove();
                element.calendar = null;
            }
        }, 100);
    };

    $ui.calendar = function (element, closeOnSelect) {
        if (element.calendar) {
            return;
        }

        let template = document.importNode(document.getElementById(calendarSettings.elementID).content, true);
        let calendar = template.firstElementChild;
        calendar.closeOnSelect = closeOnSelect;
        calendar.targetElement = element;
        calendar.evalElements = [];
        calendar.setAttribute("ui-content", "calendar");
        //calendar.setAttribute("tabindex", "0");
        //calendar.style.outline = "0";

        // - Templates
        calendar.templateDay = calendar.querySelector("[ui-template=day]");
        if (!calendar.templateDay) {
            return console.log("Template for the day element not found.");
        }
        calendar.templateDay.remove();

        calendar.templateWeek = calendar.querySelector("[ui-template=week]");
        if (!calendar.templateWeek) {
            return console.log("Template for the week element not found.");
        }
        calendar.templateWeek.target = calendar.templateWeek.parentNode;
        calendar.templateWeek.remove();

        $min.forEach(calendar, function (e) {
            if (/{{(.+)}}/.test(e.innerHTML)) {
                e.expression = e.innerHTML;
                calendar.evalElements.push(e);
            }
        });

        updateCalendar(calendar);

        element.calendar = calendar;
        if (!element.isSetup) {
            element.addEventListener("blur", function () {
                checkCalendarFocus(element);
            });
            element.isSetup = true;
        }

        let target = element.parentNode || element;
        target.appendChild(calendar);

        let windowWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
        let windowHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;

        let pos = element.getBoundingClientRect();

        if (pos.y > windowHeight / 2 && pos.y - calendar.offsetHeight > 0) {
            calendar.classList.add("top");
        }
        else {
            calendar.classList.add("bottom");
        }

        if (pos.x + (target.offsetWidth * 0.5) - (calendar.offsetWidth / 2) <= 0) {
            calendar.classList.add("right");
        }
        else if (pos.x + (target.offsetWidth * 0.5) + (calendar.offsetWidth / 2) >= windowWidth) {
            calendar.classList.add("left");
        }
        else {
            calendar.classList.add("center");
        }
    };

    $ui.calendarNextMonth = function (button) {
        let calendar = button.closest("[ui-content=calendar]");
        if (calendar.month + 1 > 11) {
            calendar.month = 0;
            calendar.year++;
        }
        else {
            calendar.month++;
        }

        checkCalendarFocus(calendar.targetElement);
        updateCalendar(calendar);
    };

    $ui.calendarPrevMonth = function (button) {
        let calendar = button.closest("[ui-content=calendar]");
        if (calendar.month - 1 < 0) {
            calendar.month = 11;
            calendar.year--;
        }
        else {
            calendar.month--;
        }

        checkCalendarFocus(calendar.targetElement);
        updateCalendar(calendar);
    };

    $ui.calendarClickDay = function (link) {
        let calendar = link.closest("[ui-content=calendar]");
        let dateValue = (link.getAttribute("[ui-template]") === "day" ? link : link.closest("[ui-template=day]")).getAttribute("date") + " 00:00";
        let element = calendar.targetElement;

        if (element.hasAttribute("onselect")) {
            eval(element.getAttribute("onselect").replace("this", "element").replace("date", "dateValue"));
        }

        let property = element.getAttribute("data-bind");
        if (property) {
            let data = {};
            data[property] = dateValue;
            $min.bind(element, data);
        }

        setTimeout(function () {
            element.calendar.remove();
            element.calendar = null;
            element.blur();
        }, 10);
    };

    // - Misc Functions
    $ui.getNextSelectValue = function (element) {
        let value = element.value || element.innerHTML;

        if (!element.select) {
            element.select = $min.findAfter(element, function (e) { return e.nodeName.toUpperCase() === "SELECT"; });
        }
        if (!element.select) { console.log("Select element not found."); return ""; }

        if (value === undefined || value === null) {
            let option = element.select.firstElementChild;
            return option ? option.innerHTML.toString() : "";
        }

        let option = $min.find(element.select, function (e) { return e.value.toString() === value.toString(); });
        return option ? option.innerHTML : value;
    };

    $ui.togglePassword = function (element, containerSelector) {
        if (!element.target) {
            element.target = element.closest(containerSelector).querySelector("input");
        }

        if (!element.target.isShow) {
            element.target.type = "text";
            $min.toggle(element, "show");
            element.target.isShow = true;
        }
        else {
            element.target.type = "password";
            $min.toggle(element, "hide");
            element.target.isShow = false;
        }
    };

    $ui.toggleValue = function (element) {
        if (element.children.length === 1 && element.children[0].nodeName.toUpperCase() === "INPUT") { return; }
        let input = element.querySelector("input");

        // - Bind Event
        if (element.nodeName.toUpperCase() === "INPUT") {
            for (let i = 0; i < element.children.length; i++) {
                element.children[i].classList.remove("active");
            }

            let active = element.querySelector("[data-value=" + input.value + "]");
            active.classList.add("active");

            return;
        }

        // - Click Event
        let previous = element.querySelector(".active");
        let next = previous ? previous.nextElementSibling || element.firstElementChild : element.firstElementChild;

        while (next.nodeName.toUpperCase() === "INPUT") {
            next = next.nextElementSibling || element.firstElementChild;
        }

        if (previous) { previous.classList.remove("active"); }
        next.classList.add("active");
        input.value = next.getAttribute("data-value");
    };

    /*
    <input type="text"
           data-url="SomeUrl/FetchData"
           onkeydown="$ui.autocomplete(this)"
           ondataselect="console.log('you selected the option'); console.log(data);" />
    <list>
        <loading>
            Loading results...
        </loading>
        <data>
            <template>
                <div>
                    <span data-bind="Name"></span>
                </div>
            </template>
        </data>
        <empty>
            No results.
        </empty>        
    </list>      
     */
    $ui.autocomplete = function (input) {
        if (!input.datalist) {
            input.datalist = input.parentNode.querySelector("list");
            if (!input.datalist) { console.log("No <list> element found for the input."); return; }
            if (!input.hasAttribute("data-url")) { console.log("No @data-url attribute found in the input."); return; }

            if (!input.property) {
                input.path = [];

                let property = input.name;
                let member;
                while (property.indexOf(".") > -1) {
                    member = property.substring(0, property.indexOf("."));
                    property = property.substring(member.length + 1);
                    input.path.push(member);
                }
                input.property = property;
            }

            //input.datalist.loading = input.datalist.querySelector("loading");
            //input.datalist.empty = input.datalist.querySelector("empty");
            input.datalist.data = input.datalist.querySelector("data");
            input.datalist.data.template = input.datalist.data.querySelector("template");

            input.addEventListener("blur", function () {
                setTimeout(function () {
                    clearTimeout(input.query);
                    input.datalist.setAttribute("state", "stopped");
                }, 200);
            });

            input.addEventListener("focus", function () {
                if (input.results) {
                    if (input.datalist.data.selected) {
                        input.datalist.data.selected.classList.remove("hover");
                    }

                    input.datalist.setAttribute("state", "loaded");
                }
            });
        }

        switch (event.key) {
            case "ArrowDown":
                {
                    if (input.datalist.data.selected) {
                        input.datalist.data.selected.classList.remove("hover");
                    }

                    input.datalist.data.selected = input.datalist.data.selected ? input.datalist.data.selected.nextElementSibling || input.datalist.data.firstElementChild : input.datalist.data.firstElementChild;
                    input.datalist.data.selected.classList.add("hover");
                    break;
                }
            case "ArrowUp":
                {
                    if (!input.datalist.data.selected) {
                        input.datalist.data.selected = null;
                    }

                    if (input.datalist.data.selected) {
                        input.datalist.data.selected.classList.remove("hover");
                    }

                    input.datalist.data.selected = input.datalist.data.selected ? input.datalist.data.selected.previousElementSibling || input.datalist.data.lastElementChild : input.datalist.data.lastElementChild;
                    input.datalist.data.selected.classList.add("hover");
                    break;
                }
            case "Enter":
                {
                    if (!input.datalist.data.selected) { return; }

                    input.value = input.datalist.data.selected.data[input.name];
                    input.datalist.setAttribute("state", "stopped");
                    input.results = null;

                    if (input.hasAttribute("ondataselect")) {
                        eval(input.getAttribute("ondataselect").replace("this", "input").replace("data", "input.datalist.data.selected.data"));
                    }
                    break;
                }
            case "Tab":
                { break; }
            default:
                {
                    input.results = null;

                    clearTimeout(input.query);
                    input.query = setTimeout(function () {
                        if (!input.value) { return; }

                        let position = $min.getPosition(input, input.parentNode);
                        input.datalist.style.top = position.top + position.height + "px";
                        input.datalist.style.left = position.left + "px";
                        input.datalist.style.width = position.width + "px";

                        let request = { Data: {} };
                        let current = request.Data;
                        for (let i = 0; i < input.path.length; i++) {
                            if (!current[input.path[i]]) {
                                current[input.path[i]] = {};
                            }
                            current = current[input.path[i]];
                        }
                        current[input.property] = input.value;

                        //if (input.hasAttribute("data-filter")) {
                        //    eval(input.getAttribute("data-filter").replace("this", "input"));
                        //}

                        $min.clear(input.datalist.data);
                        input.datalist.setAttribute("state", "loading");

                        $min.ajax({
                            url: $min.root() + $ui.relativePath + input.getAttribute("data-url"),
                            content: request,
                            headers: {
                                "Content-Type": "application/json",
                                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                            },
                            onsuccess: function (response) {
                                if (response.Code !== 0) {
                                    input.datalist.setAttribute("state", "stopped");
                                    return $ui.message(response.Message, "error");
                                }

                                if (!response.Data.length) {
                                    input.datalist.setAttribute("state", "empty");
                                    return;
                                }

                                input.results = response.Data;
                                input.datalist.setAttribute("state", "loaded");

                                for (let i = 0; i < response.Data.length; i++) {
                                    let template = document.importNode(input.datalist.data.template.content, true);
                                    template.firstElementChild.data = response.Data[i];
                                    $min.bind(template, response.Data[i]);
                                    input.datalist.data.appendChild(template);
                                }
                            },
                            onfailure: function (httpCode, httpMessage) {
                                if (httpCode === 401) {
                                    let currentURL = window.location.href.replace($min.root() + $ui.relativePath, "");
                                    window.location.href = $min.root() + $ui.relativePath + $ui.redirectLoginURL + currentURL;
                                }

                                $ui.message(httpMessage, "error");
                            }
                        });
                    }, 500);
                    break;
                }
        }
    };

    var dataHistory = [];
    $ui.setPage = function (pages, template) {
        let currentPage = pages.querySelector(".page");
        if (currentPage) {
            currentPage.remove();
        }

        let page = document.importNode(template.content, true).firstElementChild;

        pages.appendChild(page);
    };

    $ui.newPage = function (template) {
        if (typeof template === "string") {
            template = document.getElementById(template);
        }

        let page = document.importNode(template.content, true).firstElementChild;
        page.classList.add("page");

        return page;
    };

    $ui.loadPage = function (url, callback) {
        $min.ajax({
            url: $min.root() + "Page/" + url,
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                let dummy = document.createElement("div");
                dummy.innerHTML = response;

                let page = dummy.firstElementChild;
                page.classList.add("page");

                callback(page);
                window.history.pushState("", document.title, $min.root() + url);
            },
            onfailure: function (httpCode, httpMessage) {
                console.log(httpCode + " : " + httpMessage);
            }
        });

        let page = document.createElement("div");

        return page;
    };

    $ui.slidePagesLeft = function (pages, newPage, callback) {
        let currentPage = pages.querySelector(".page");
        if (!currentPage) { return; }

        pages.appendChild(newPage);
        currentPage.classList.add("left");

        if (callback) {
            callback(currentPage, newPage);
        }

        setTimeout(function () {
            currentPage.remove();
        }, 500);
    };

    $ui.slidePagesRight = function (pages, newPage, callback) {
        let currentPage = pages.querySelector(".page");
        if (!currentPage) { return; }

        pages.appendChild(newPage);
        currentPage.classList.add("right");

        if (callback) {
            callback(currentPage, newPage);
        }

        setTimeout(function () {
            currentPage.remove();
        }, 500);
    };

    $ui.bindPagesLeft = function (element, dataSelector, pages, newPage, callback) {
        let data = dataSelector ? element.closest(dataSelector) : null;
        if (data) {
            $min.bind(newPage, data.data);
            newPage.data = data.data;
        }

        $ui.slidePagesLeft(pages, newPage, callback);
    };

    $ui.bindPagesRight = function (element, dataSelector, pages, newPage, callback) {
        let data = dataSelector ? element.closest(dataSelector) : null;
        if (data) {
            $min.bind(newPage, data.data);
            newPage.data = data.data;
        }

        $ui.slidePagesRight(pages, newPage, callback);
    };

    (function ($pages) {
        $pages.expandFromElement = function (pages, page, element) {
            let currentPage = pages.querySelector(".page");

            let dimensions = $min.getPosition(element, pages);
            page.style.left = dimensions.left + "px";
            page.style.top = dimensions.top + "px";
            page.style.width = dimensions.width + "px";
            page.style.height = dimensions.height + "px";
            page.original = dimensions;

            pages.appendChild(page);
            page.classList.add("cover");

            //let scripts = page.querySelectorAll("script");        
            //for (let i = 0; i < scripts.length; i++) {
            //    eval(scripts[i].innerText);
            //}

            setTimeout(function () {
                page.removeAttribute("style");
            }, 1);

            setTimeout(function () {
                currentPage.remove();
            }, 500);
        };

        $pages.contractFromElement = function (pages, page, element) {
            pages.insertBefore(page, element);
            element.classList.add("cover");

            setTimeout(function () {
                if (element.original) {
                    element.style.left = element.original.left + "px";
                    element.style.top = element.original.top + "px";
                    element.style.width = element.original.width + "px";
                    element.style.height = element.original.height + "px";
                }
                else {
                    element.style.left = "50%";
                    element.style.top = "50%";
                    element.style.width = 0;
                    element.style.height = 0;
                }
                element.style.opacity = 0;
            }, 1);

            setTimeout(function () {
                element.remove();
            }, 500);
        };

        $pages.previous = function (button, templateID, selectorForPage, selectorForData) {
            let pages = button.closest(selectorForPage);
            let page = $ui.$pages.fromTemplate(document.getElementById(templateID));

            let html = selectorForData ? button.closest(selectorForData) : null;
            if (html) {
                $min.bind(page, html.data);
                page.data = html.data;
            }

            $ui.$pages.slideRight(pages, page);
        };

        $pages.next = function (button, templateID, selectorForPage, selectorForData) {
            let pages = button.closest(selectorForPage);
            let page = $ui.$pages.fromTemplate(document.getElementById(templateID));

            let html = selectorForData ? button.closest(selectorForData) : null;
            if (html) {
                $min.bind(page, html.data);
                page.data = html.data;
            }

            $ui.$pages.slideLeft(pages, page);
        };
    })($ui.$pages = $ui.$pages || {});

    // - TODO: Update    

    $ui.lista = function (input, property, url) {
        if (input.isBusy || !input.list) { return; }
        if (!input.property) {
            input.path = [];

            let member;
            while (property.indexOf(".") > -1) {
                member = property.substring(0, property.indexOf("."));
                property = property.substring(member.length + 1);
                input.path.push(member);
            }
            input.property = property;
        }

        if (!input.value) {
            clearTimeout(input.load);
            input.data = "";
            return;
        }

        let option = $min.find(input.list, function (e) { return e.value === input.value; });
        if (option) {
            clearTimeout(input.load);
            input.data = option.data;
            return;
        }

        clearTimeout(input.load);
        input.data = "";
        input.load = setTimeout(function () {
            let data = {};
            let current = data;
            for (let i = 0; i < input.path.length; i++) {
                if (!current[input.path[i]]) {
                    current[input.path[i]] = {};
                }
                current = current[input.path[i]];
            }
            current[input.property] = input.value;

            input.isBusy = true;

            $min.clear(input.list);
            $min.ajax({
                url: $min.root() + url,
                content: { Data: data, Page: 1, PerPage: 5 },
                headers: {
                    "Content-Type": "application/json",
                    "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
                },
                onsuccess: function (response) {
                    console.log("?");
                    input.isBusy = false;
                    if (response.Code !== 0) {
                        return $ui.message(response.Message, "error");
                    }

                    for (let i = 0; i < response.Data.length; i++) {
                        let current = response.Data[i];
                        for (let j = 0; j < input.path.length; j++) {
                            current = current[input.path[j]];
                            if (!current) { break; }
                        }
                        if (!current) { continue; }

                        if (response.Data.length === 1 && input.value === current[input.property]) {
                            input.data = response.Data[i];
                            return;
                        }

                        let option = document.createElement("option");
                        option.value = current[input.property];
                        option.data = response.Data[i];
                        input.list.appendChild(option);
                    }
                },
                onfailure: function (response) {
                    input.isBusy = false;
                    $ui.message(response, "error");
                }
            });
        }, 500);
    };

    $ui.getListValue = function (button, list, propertyID) {
        if (!button.input) {
            button.input = $min.findBefore(button, function (e) { return e.type; });
            button.list = $min.findAfter(button, function (e) { return e.getAttribute("data-array") === list; });
        }

        if (!button.input.data) {
            $min.bind(button.input.parentNode, { Error: button.input.getAttribute("list-required-error") || "No data selected." }, "error");
            return;
        }

        if (button.list.querySelector("[name=\"" + propertyID + "\"][value=\"" + button.input.data[propertyID] + "\"]")) {
            $min.bind(button.input.parentNode, { Error: button.input.getAttribute("list-repeated-error") || "Data already saved." }, "error");
            return;
        }

        let template = document.importNode((button.list.template || button.list.firstElementChild).content, true);
        $min.bind(template, button.input.data);
        button.list.appendChild(template);
    };

    $ui.clear = function (select, value) {
        if (list[select.name]) {
            for (let i = 0; i < list[select.name].values.length; i++) {
                if (list[select.name].values[i][value]) {
                    delete list[select.name].values[i][value];
                }
            }
        }
    };
})(window.$ui = window.$ui || {});

(function ($user) {
    var currentUser = false;
    var loginCodeFunctions = [];

    $user.onLoginCode = function (code, callback) {
        loginCodeFunctions[code] = callback;
    };

    $user.onForgot = false;

    $user.login = function (element, selector) {

        let form = selector ? element.closest(selector) : element;
        if (form.isBusy) { return false; }

        let data = $min.read(form, form.data || {});

        form.isBusy = true;
        form.setAttribute("state", "loading");

        $min.ajax({
            url: form.action || form.href,
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                form.isBusy = false;
                form.removeAttribute("state");
                if (loginCodeFunctions[response.Code] && !loginCodeFunctions[response.Code](form, response)) {
                    return;
                }

                if (response.Code !== 0) {
                    $ui.message(response.Message, "error");
                    $min.bind(form, response.Data, "error");
                    return;
                }

                let userInfo = document.querySelectorAll("[ui-content=user-info]");
                if (userInfo) {
                    for (let i = 0; i < userInfo.length; i++) {
                        $min.bind(userInfo[i], response.Data);
                    }
                }

                currentUser = response.Data;

                $ui.message(response.Message, "success");
            },
            onfailure: function (httpCode, httpMessage) {
                form.isBusy = false;
                form.removeAttribute("state");

                $ui.message(httpMessage, "error");
            }
        });

        return false;
    };

    $user.logout = function (element, selector) {
        let form = selector ? element.closest(selector) : element;
        if (form.isBusy) { return false; }

        let data = $min.read(form, form.data || {});

        form.isBusy = true;
        $min.ajax({
            url: form.action || form.href,
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                form.isBusy = false;

                window.location.href = $min.root() + $ui.relativePath + response;
            },
            onfailure: function (httpCode, httpMessage) {
                form.isBusy = false;

                $ui.message(httpMessage, "error");
            }
        });

        return false;
    };

    $user.forgot = function (element, selector) {
        let form = selector ? element.closest(selector) : element;
        if (form.isBusy) { return false; }

        let data = $min.read(form, form.data || {});

        form.isBusy = true;
        $min.ajax({
            url: form.action || form.href,
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                form.isBusy = false;

                if (response.Code !== 0) {
                    $min.bind(form, response.Data, "error");
                    return;
                }

                if ($user.onForgot) {
                    $user.onForgot(form, response);
                }

                //$ui.message(response.Message, "success");
            },
            onfailure: function (httpCode, httpMessage) {
                form.isBusy = false;

                $ui.message(httpMessage, "error");
            }
        });

        return false;
    };

    $user.toggleMenu = function (input) {
        document.cookie = "navmenu=" + (input.checked ? "closed" : "open") + ";path=/";
    };

    $user.addRole = function (element) {
        let cell = element.closest(".relative");
        let roles = cell.querySelector("[data-array='Perfis']");

        if (!roles.template) { roles.template = roles.querySelector(":scope > template"); }

        let template = document.importNode(roles.template.content, true);
        $min.bind(template, element.data);
        roles.appendChild(template);

        element.parentNode.parentNode.classList.add("hidden");
        element.remove();
    };
})(window.$user = window.$user || {});

(function ($chat) {
    var updateTimerMS = 10000;
    var lastUpdate = false;
    var userList = {};
    var chatElement = false;

    var userUpdaterInterval = false;
    var userUpdaterIsBusy = false;
    var userUpdater = function () {
        if (userUpdaterIsBusy) { return; }

        let data = {
            PerPage: userList.count || 0
        };

        $min.ajax({
            url: $min.root() + "Api/Usuarios/Mensagem",
            content: data,
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                userUpdaterIsBusy = false;

                // - TODO: Contar os que sumiram

                if (response.Total === userList.count) { return; }
                if (!response.Data) { return; }

                userList.count = response.Total;
                for (let i = 0; i < response.Data.length; i++) {
                    if (!userList[response.Data[i].UsuarioID]) {
                        userList[response.Data[i].UsuarioID] = { Messages: [] };
                    }

                    userList[response.Data[i].UsuarioID].Nome = response.Data[i].Nome;
                }

                $min.bind(chatElement.users, { Usuarios: response.Data });
            },
            onfailure: function (httpCode, httpMessage) {
                userUpdaterIsBusy = false;
            }
        });
    };

    var messageUpdaterInterval = false;
    var messageUpdaterIsBusy = false;
    var messageUpdater = function () {
        if (messageUpdaterIsBusy) { return; }

        let data = {
            Timestamp: lastUpdate ? lastUpdate : null
        };

        $min.ajax({
            url: $min.root() + "Api/Checar/Mensagem",
            content: data,
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                messageUpdaterIsBusy = false;
                lastUpdate = response.Timestamp;

                console.log(response);

                for (let i = 0; i < response.Data.length; i++) {
                    if (!userList[response.Data[i].UsuarioID]) {
                        userList[response.Data[i].UsuarioID] = { Messages: [] };
                    }

                    userList[response.Data[i].UsuarioID].Messages.push(response.Data[i]);
                }
            },
            onfailure: function (httpCode, httpMessage) {
                messageUpdaterIsBusy = false;
            }
        });
    };

    $chat.start = function () {
        if ($chat.updater) { return; }

        chatElement = document.querySelector("chat");
        chatElement.users = chatElement.querySelector("[data-element=chat-users]");

        userUpdater();
        userUpdaterInterval = setInterval(userUpdater, updateTimerMS);
        messageUpdaterInterval = setInterval(messageUpdater, updateTimerMS);
    };

    $chat.stop = function () {
        clearInterval(userUpdaterInterval);
        clearInterval(messageUpdaterInterval);

        userUpdaterIsBusy = false;
        messageUpdaterIsBusy = false;
    };

    $chat.log = function () {
        console.log(userList);
    };

    $chat.open = function (link) {
        link.closest("chat").querySelector("[type=checkbox]").click();
    };
})(window.$chat = window.$chat || {});

(function ($timesheet) {
    var header = false;
    var totalHoras = false;
    var inputUsuario = false;
    var selectTimesheets = false;
    var selectGestores = false;
    var popupAtividade = false;
    var popupResumo = false;
    var timesheet = false;
    var pasteboard = false;
    var cliente = false;

    var diasSemana = [
        "Domingo",
        "Segunda-feira",
        "Terça-feira",
        "Quarta-feira",
        "Quinta-feira",
        "Sexta-feira",
        "Sábado"
    ];

    var meses = [
        "Janeiro",
        "Fevereiro",
        "Março",
        "Abril",
        "Maio",
        "Junho",
        "Julho",
        "Agosto",
        "Setembro",
        "Outubro",
        "Novembro",
        "Dezembro"
    ];

    $timesheet.relativePath = "";

    var init = function (element) {
        if (element.isInit) { return; }

        header = element.querySelector("[ts-content='header']");
        totalHoras = element.querySelector("[ts-content='total']");
        inputUsuario = element.querySelector("[name=ColaboradorID]");
        selectTimesheets = element.querySelector("[name=TimesheetID]");
        selectGestores = element.querySelector("[name=ContatoID]");
        selectGestores.defaultOptions = [];
        for (let i = 0; i < selectGestores.options.length; i++) {
            selectGestores.defaultOptions.push(selectGestores.options[i]);
        }

        popupAtividade = element.querySelector("[ts-content='atividade-popup']");
        popupResumo = element.querySelector("[ts-content='resumo-popup']")
        timesheet = element.querySelector("[ts-content='timesheet']");

        element.isInit = true;
    };

    $timesheet.carregarTimesheets = function (container) {
        if (container) { init(container); }

        if (!inputUsuario.value) {
            timesheet.setAttribute("state", "initial");

            cliente = false;
            $min.clear(selectTimesheets);
            return;
        }

        if (timesheet.isOcupado) { return; }

        let data = $min.read(header);

        timesheet.isOcupado = true;
        timesheet.setAttribute("state", "loading");

        $min.ajax({
            url: $min.root() + $timesheet.relativePath + "Api/Calendario/Carregar",
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                timesheet.isOcupado = false;
                timesheet.setAttribute("state", "initial");

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                cliente = response.Data.Cliente;

                if (response.Data.Gestores.length > 0) {
                    $min.clear(selectGestores);

                    for (let i = 0; i < selectGestores.defaultOptions.length; i++) {
                        selectGestores.appendChild(selectGestores.defaultOptions[i]);
                    }

                    for (let i = 0; i < response.Data.Gestores.length; i++) {
                        let option = document.createElement("option");
                        option.value = response.Data.Gestores[i].ContatoID;
                        option.innerHTML = response.Data.Gestores[i].Nome;

                        let selectedValue = selectGestores.value || selectGestores.queuedValue;
                        if (selectedValue === response.Data.Gestores[i].GestorID) {
                            option.selected = true;
                        }

                        selectGestores.appendChild(option);
                    }
                }

                if (response.Data.Timesheets.length > 0) {
                    $min.clear(selectTimesheets);

                    for (let i = 0; i < response.Data.Timesheets.length; i++) {
                        let option = document.createElement("option");
                        option.value = response.Data.Timesheets[i].TimesheetID;
                        option.innerHTML = meses[response.Data.Timesheets[i].Mes - 1] + "/" + response.Data.Timesheets[i].Ano;
                        option.data = response.Data.Timesheets[i];

                        selectTimesheets.appendChild(option);
                    }
                }

                $timesheet.carregarAtividades();
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $timesheet.relativePath, "");
                    window.location.href = $min.root() + $timesheet.relativePath + "Sistema/Login?ReturnURL=" + currentURL;
                }

                timesheet.isOcupado = false;
                timesheet.setAttribute("state", "initial");

                $ui.message(httpMessage, "error");
            }
        });
    };

    $timesheet.carregarAtividades = function () {
        if (timesheet.isOcupado) { return; }

        let data = selectTimesheets.options[selectTimesheets.selectedIndex].data;
        data.ColaboradorID = inputUsuario.value;

        popupAtividade.classList.add("hidden");
        popupAtividade.atividade = null;

        timesheet.isOcupado = true;
        timesheet.setAttribute("state", "loading");

        $min.ajax({
            url: $min.root() + $timesheet.relativePath + "Api/Calendario/CarregarAtividades",
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                timesheet.isOcupado = false;
                timesheet.setAttribute("state", "loaded");

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                $min.bind(header, response.Data);
                $min.bind(timesheet, response.Data);

                //console.log(response);

                // - Timesheet fechado.
                if (response.Data.Status === 0) {
                    timesheet.isFechado = true;
                    timesheet.classList.add("fechado");
                    selectGestores.disabled = true;

                    $min.forEach(popupAtividade, function (e) {
                        if (!e.name) { return; }
                        e.disabled = true;
                    });
                }
                else {
                    timesheet.isFechado = false;
                    timesheet.classList.remove("fechado");
                    selectGestores.disabled = false;

                    $min.forEach(popupAtividade, function (e) {
                        if (!e.name) { return; }
                        e.disabled = false;
                    });
                }

                $min.calculate(timesheet);
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $timesheet.relativePath, "");
                    window.location.href = $min.root() + $timesheet.relativePath + "Sistema/Login?ReturnURL=" + currentURL;
                }

                timesheet.isOcupado = false;
                timesheet.setAttribute("state", "initial");
                $ui.message(httpMessage, "error");
            }
        });
    };

    var cellWidth = 4.166666666666667;

    var posicionarPopup = function (atividade) {
        popupAtividade.style.display = "block";

        let pos = atividade.getBoundingClientRect();
        let windowHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;

        if (pos.y < windowHeight / 2) {
            popupAtividade.classList.add("bottom");
            popupAtividade.classList.remove("top");
            popupAtividade.style.top = (atividade.offsetY + pos.height) + "px";
        }
        else {
            popupAtividade.classList.add("top");
            popupAtividade.classList.remove("bottom");
            popupAtividade.style.top = (atividade.offsetY - popupAtividade.offsetHeight) + "px";
        }

        popupAtividade.style.left = (pos.left + pos.width / 2) + "px";
    };

    $timesheet.adicionarAtividade = function (element) {
        if (timesheet.isFechado) { return; }

        let template = document.importNode(element.template.content, true);
        let atividade = template.firstElementChild;
        atividade.isTemporario = true;
        element.appendChild(template);

        let current = atividade;
        let offsetX = atividade.offsetLeft;
        let offsetY = atividade.offsetTop;

        while (current = current.offsetParent) {
            if (current.nodeName.toUpperCase() === "MAIN") { break; }

            offsetX += current.offsetLeft;
            offsetY += current.offsetTop;
        }

        let relativeX = event.clientX - offsetX;
        let targetHour = Math.floor(relativeX * 24 / element.offsetWidth);
        if (targetHour === 24) { targetHour--; }

        atividade.offsetX = offsetX;
        atividade.offsetY = offsetY;
        atividade.style.left = targetHour * cellWidth + "%";
        atividade.style.right = (targetHour === 23 ? 0 : 100 - (targetHour + 1) * cellWidth) + "%";

        posicionarPopup(atividade);

        if (popupAtividade.atividade && popupAtividade.atividade.isTemporario) {
            popupAtividade.atividade.remove();
            $min.calculate(timesheet);
        }
        popupAtividade.atividade = atividade;

        let entrada = popupAtividade.querySelector("[name=HoraEntrada]");
        entrada.value = targetHour.toString().padStart(2, '0') + ":00";
        $timesheet.atualizarAtividade(entrada);

        let saida = popupAtividade.querySelector("[name=HoraSaida]");
        saida.value = (targetHour === 23 ? 0 : targetHour + 1).toString().padStart(2, '0') + ":00";
        $timesheet.atualizarAtividade(saida);

        let observacoes = popupAtividade.querySelector("[name=Descricao]");
        observacoes.value = "";

        let atividadeID = popupAtividade.querySelector("[name=AtividadeID]");
        atividadeID.value = "0";

        //popupAtividade.setAttribute("state", "new");

        //popupAtividade.querySelector("button[button-action='cancel']").classList.remove("hidden");
        //popupAtividade.querySelector("button[button-action='delete']").classList.add("hidden");
        $min.validate(popupAtividade);
    };

    $timesheet.abrirAtividade = function (atividade) {
        if (popupAtividade.atividade && popupAtividade.atividade.isTemporario) {
            popupAtividade.atividade.remove();
            $min.calculate(timesheet);
        }

        if (popupAtividade.atividade === atividade) {
            popupAtividade.atividade = null;
            popupAtividade.style.removeProperty("display");
            event.stopPropagation();
            return;
        }

        popupAtividade.atividade = atividade;

        let current = atividade;
        let offsetX = atividade.offsetLeft;
        let offsetY = atividade.offsetTop;
        //let maxHeight = 0;
        while (current = current.offsetParent) {
            if (current.nodeName.toUpperCase() === "MAIN") { break; }

            offsetX += current.offsetLeft;
            offsetY += current.offsetTop;
            //maxHeight = current.scrollHeight > maxHeight ? current.scrollHeight : maxHeight;
        }

        atividade.offsetX = offsetX;
        atividade.offsetY = offsetY;

        posicionarPopup(atividade);

        $min.bind(popupAtividade, atividade.data);
        $min.validate(popupAtividade);
        event.stopPropagation();
    };

    $timesheet.bindAtividade = function (element, data) {
        element.data = data;
        element.style.left = parseFloat(data.HoraEntrada.substr(0, 2)) * cellWidth + parseFloat(data.HoraEntrada.substr(3, 2)) * cellWidth / 60 + "%";
        element.style.right = data.HoraSaida.substr(0, 2) === "00" ? 0 : (24 - parseFloat(data.HoraSaida.substr(0, 2))) * cellWidth - parseFloat(data.HoraSaida.substr(3, 2)) * cellWidth / 60 + "%";
        //element.style.width = (data.Ate - data.De) / 3600000 * cellWidth + "%";
        element.classList.add("tooltip");
    };

    $timesheet.bindWeekday = function (element, data) {
        let date = new Date(data.Ano + "-" + data.Mes + "-" + data.Dia + " 00:00");
        //if (!date) { return; }

        element.innerHTML = diasSemana[date.getDay()];

        if (date.getDay() === 0 || date.getDay() === 6) {
            element.closest(".day").classList.add("weekend");
        }
    };

    $timesheet.bindDay = function (element, dia) {
        element.innerHTML = dia.toString().padStart(2, '0');
    };

    $timesheet.bindMonth = function (element, mes) {
        element.innerHTML = mes.toString().padStart(2, '0');
    };

    $timesheet.bindHoliday = function (element, feriado) {
        if (feriado) {
            element.classList.remove("hidden");
            element.closest(".day").classList.add("holiday");
        }
    };

    $timesheet.atualizarAtividade = function (element) {
        if (!element.value) { return; }

        if (element.name === "HoraEntrada") {
            popupAtividade.atividade.style.left = parseFloat(element.value.substr(0, 2)) * cellWidth + parseFloat(element.value.substr(3, 2)) * cellWidth / 60 + "%";
            popupAtividade.atividade.querySelector("[name=HoraEntrada]").value = element.value.substr(0, 2) + ":" + element.value.substr(3, 2);
        }

        if (element.name === "HoraSaida") {
            popupAtividade.atividade.style.right = element.value.substr(0, 2) === "00" ? 0 : 100 - parseFloat(element.value.substr(0, 2)) * cellWidth - parseFloat(element.value.substr(3, 2)) * cellWidth / 60 + "%";
            popupAtividade.atividade.querySelector("[name=HoraSaida]").value = element.value.substr(0, 2) + ":" + element.value.substr(3, 2);
        }

        $min.calculate(timesheet);
    };

    $timesheet.removerAtividade = function () {
        //if (popupAtividade.atividade.isTemporario) {
        let atividadeID = popupAtividade.atividade.querySelector("[name=AtividadeID]").value;
        if (atividadeID == false) { // - 0 ou "" ou undefined
            popupAtividade.atividade.remove();
            $min.calculate(timesheet);
        }
        else {
            popupAtividade.atividade.querySelector("[name=HoraEntrada]").value = "";
            popupAtividade.atividade.querySelector("[name=HoraSaida]").value = "";
            popupAtividade.atividade.querySelector("[name=State]").value = 2;
            popupAtividade.atividade.style.display = "none";
        }

        popupAtividade.atividade = null;
        popupAtividade.style.removeProperty("display");

        $min.calculate(timesheet);
    };

    $timesheet.confirmarAtividade = function () {
        if (!$min.validate(popupAtividade, true)) { return; }

        let data = $min.read(popupAtividade);
        data.State = 1;
        $min.bind(popupAtividade.atividade, data);

        popupAtividade.atividade.isTemporario = false;
        popupAtividade.atividade = null;
        popupAtividade.style.removeProperty("display");
    };

    $timesheet.validarAtividade = function (input) {
        let entradaAtual = getTime($min.findBefore(input, function (e) { return e.name === "HoraEntrada"; }).value);
        let saidaAtual = getTime(input.value);
        if (saidaAtual === 0) { saidaAtual = 86400000; }

        if (saidaAtual <= entradaAtual) {
            input.setAttribute("onvalidation-error", input.getAttribute("onvalidation-invalido-error"));
            return false;
        }

        // - Conferindo se não conflita com nenhuma outra atividade
        for (let i = 0; i < popupAtividade.atividade.parentNode.childNodes.length; i++) {
            if (popupAtividade.atividade.parentNode.childNodes[i].nodeType !== 1 || popupAtividade.atividade.parentNode.childNodes[i] === popupAtividade.atividade) { continue; }

            let atividade = popupAtividade.atividade.parentNode.childNodes[i];
            let entrada = getTime(atividade.querySelector("[name=HoraEntrada]").value);
            let saida = getTime(atividade.querySelector("[name=HoraSaida]").value);

            if (saidaAtual > entrada && entradaAtual < saida) {
                input.setAttribute("onvalidation-error", input.getAttribute("onvalidation-conflito-error"));
                return false;
            }
        }

        return true;
    };

    $timesheet.copiarAtividades = function (element) {
        if (timesheet.isFechado === true) { return; }

        if (!element.day) {
            element.day = element.closest("div").previousElementSibling;
        }

        let data = {
            Atividades: $min.read(element.day)
        };

        for (let i = 0; i < data.Atividades.length; i++) {
            if (data.Atividades[i].HoraEntrada === undefined || data.Atividades[i].HoraSaida === undefined) {
                data.Atividades.remove(i);
                i--;
                continue;
            }

            data.Atividades[i].AtividadeID = 0;
            data.Atividades[i].State = 1;
        }

        pasteboard = data;
    };

    $timesheet.colarAtividades = function (element) {
        if (timesheet.isFechado === true) { return; }

        if (!element.day) {
            element.day = element.closest("div").previousElementSibling;
        }

        if (pasteboard && pasteboard.Atividades && pasteboard.Atividades.length > 0) {
            let atividades = element.day.querySelectorAll("[name=State]");
            for (let i = 0; i < atividades.length; i++) {
                atividades[i].parentNode.style.display = "none"; //classList.add("hidden");
                atividades[i].value = 2;
            }

            //$min.bind(element.day, pasteboard || {});
            for (let i = 0; i < pasteboard.Atividades.length; i++) {
                let template = document.importNode(element.day.closest("[data-array]").template.content, true);
                $min.bind(template, pasteboard.Atividades[i]);
                element.day.appendChild(template);
            }

            $min.calculate(timesheet);
        }

        popupAtividade.atividade = null;
        popupAtividade.style.removeProperty("display");
    };

    $timesheet.salvar = function (button) {
        if (timesheet.isOcupado) { return false; }
        if (!$min.validate(timesheet)) { return false; }

        let timesheetAtual = selectTimesheets.options[selectTimesheets.selectedIndex].data;
        let data = {
            ColaboradorID: inputUsuario.value,
            ContatoID: selectGestores.value,
            PeriodoDe: timesheetAtual.PeriodoDe,
            PeriodoAte: timesheetAtual.PeriodoAte,
            Mes: timesheetAtual.Mes,
            Ano: timesheetAtual.Ano,
            Status: timesheetAtual.Status
        };
        $min.read(timesheet, data);

        //console.log(data);

        button.disabled = true;
        timesheet.isOcupado = true;
        $min.ajax({
            url: $min.root() + $timesheet.relativePath + "Api/Calendario/Salvar",
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                button.disabled = false;
                timesheet.isOcupado = false;
                //if (loading) { loading.classList.add("hidden"); }

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                $min.bind(timesheet, response.Data);
                $min.calculate(timesheet);
                $ui.message("Timesheet atualizado.", "success");
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $timesheet.relativePath, "");
                    window.location.href = $min.root() + $timesheet.relativePath + "Sistema/Login?ReturnURL=" + currentURL;
                }

                button.disabled = false;
                timesheet.isOcupado = false;
                $ui.message(httpMessage, "error");
            }
        });
    };

    $timesheet.fechar = function () {
        let popup = $min.popup(popupResumo);

        let usuario = inputUsuario.options[inputUsuario.selectedIndex];
        let gestor = selectGestores.options[selectGestores.selectedIndex];
        let timesheetAtual = selectTimesheets.options[selectTimesheets.selectedIndex];

        let data = {
            ColaboradorID: usuario.value,
            Nome: usuario.innerHTML,
            RazaoSocial: cliente.RazaoSocial,
            ContatoID: gestor.value,
            Gestor: gestor.value ? gestor.innerHTML : "-",
            Mes: timesheetAtual.data.Mes,
            Ano: timesheetAtual.data.Ano,
            PeriodoDe: timesheetAtual.data.PeriodoDe,
            PeriodoAte: timesheetAtual.data.PeriodoAte,
            DataReferencia: timesheetAtual.innerHTML,
            TotalHoras: totalHoras.innerHTML,
            Datas: $min.read(timesheet).Datas,
            Alertas: []
        };

        for (let i = 0; i < data.Datas.length; i++) {
            let diaHoras = 0;
            let temMotivo = false;
            for (let j = 0; j < data.Datas[i].Atividades.length; j++) {
                diaHoras += $min.stringToMilliseconds(data.Datas[i].Atividades[j].HoraSaida) - $min.stringToMilliseconds(data.Datas[i].Atividades[j].HoraEntrada);
                temMotivo = temMotivo || data.Datas[i].Atividades[j].Descricao ? true : false;
            }

            let dia = new Date(data.Datas[i].Ano + "-" + data.Datas[i].Mes + "-" + data.Datas[i].Dia + " 00:00");
            let semana = dia.getDay();

            // - Finais de Semana ou Feriados sem motivo
            if (diaHoras > 0 && (semana === 0 || semana === 6 || data.Datas[i].Feriado) && !temMotivo) {
                data.Alertas.push({
                    Data: dia,
                    Total: $min.millisecondsToString(diaHoras),
                    Motivo: "Horas Extras Sem Motivo"
                });
            }
            // - Menos de 8h sem motivo
            else if (diaHoras < 28800000 && semana > 0 && semana < 6 && !data.Datas[i].Feriado && !temMotivo) {
                data.Alertas.push({
                    Data: dia,
                    Total: $min.millisecondsToString(diaHoras),
                    Motivo: "Menos de 8h Sem Motivo"
                });
            }
            else if (diaHoras > 28800000 && semana > 0 && semana < 6 && !data.Datas[i].Feriado && !temMotivo) {
                data.Alertas.push({
                    Data: dia,
                    Total: $min.millisecondsToString(diaHoras),
                    Motivo: "Mais de 8h Sem Motivo"
                });
            }
        }

        popup.data = data;
        $min.bind(popup, data);
    };

    $timesheet.fecharResumo = function (element) {
        element.closest(".ui-popup").close();
    };

    $timesheet.salvarResumo = function (element) {
        let popup = element.closest(".ui-popup");
        //if (!$min.validate(popup)) { return; }

        let data = popup.data;
        data.Status = 0;

        timesheet.isOcupado = true;
        $min.ajax({
            url: $min.root() + $timesheet.relativePath + "Api/Calendario/Salvar",
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                timesheet.isOcupado = false;

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                timesheet.classList.add("fechado");
                timesheet.isFechado = true;
                selectGestores.disabled = true;

                $min.forEach(popupAtividade, function (e) {
                    if (!e.name) { return; }
                    e.disabled = true;
                });

                popup.close();
                $ui.message("Timesheet fechado.", "success");
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $timesheet.relativePath, "");
                    window.location.href = $min.root() + $timesheet.relativePath + "Sistema/Login?ReturnURL=" + currentURL;
                }

                timesheet.isOcupado = false;
                //if (loading) { loading.classList.add("hidden"); }
                $ui.message(httpMessage, "error");
            }
        });
    };

    $timesheet.abrir = function (button) {
        let usuario = inputUsuario.options[inputUsuario.selectedIndex];
        let gestor = selectGestores.options[selectGestores.selectedIndex];
        let timesheetAtual = selectTimesheets.options[selectTimesheets.selectedIndex];

        let data = {
            ColaboradorID: usuario.value,
            Nome: usuario.innerHTML,
            RazaoSocial: cliente.RazaoSocial,
            ContatoID: gestor.value,
            Gestor: gestor.value ? gestor.innerHTML : "-",
            Mes: timesheetAtual.data.Mes,
            Ano: timesheetAtual.data.Ano,
            PeriodoDe: timesheetAtual.data.PeriodoDe,
            PeriodoAte: timesheetAtual.data.PeriodoAte,
            DataReferencia: timesheetAtual.innerHTML
        };
        data.Status = 1;

        button.disabled = true;
        timesheet.isOcupado = true;
        $min.ajax({
            url: $min.root() + $timesheet.relativePath + "Api/Calendario/Salvar",
            content: { Data: data },
            headers: {
                "Content-Type": "application/json",
                "X-XSRF-TOKEN": $min.getCookie("XSRF-TOKEN")
            },
            onsuccess: function (response) {
                button.disabled = false;
                timesheet.isOcupado = false;

                if (response.Code !== 0) {
                    return $ui.message(response.Message, "error");
                }

                timesheet.classList.remove("fechado");
                timesheet.isFechado = false;
                selectGestores.disabled = true;

                $min.forEach(popupAtividade, function (e) {
                    if (!e.name) { return; }
                    e.disabled = false;
                });

                $ui.message("Timesheet aberto.", "success");
            },
            onfailure: function (httpCode, httpMessage) {
                if (httpCode === 401) {
                    let currentURL = window.location.href.replace($min.root() + $timesheet.relativePath, "");
                    window.location.href = $min.root() + $timesheet.relativePath + "Sistema/Login?ReturnURL=" + currentURL;
                }

                button.disabled = false;
                timesheet.isOcupado = false;
                //if (loading) { loading.classList.add("hidden"); }
                $ui.message(httpMessage, "error");
            }
        });
    };

    $timesheet.checarTotal = function (element, total) {
        if (!element.extra) {
            element.extra = element.nextElementSibling;
        }

        let hours = parseFloat(total.substr(0, 2));
        let minutes = total.substr(3, 2);

        let day = element.closest(".day");
        let date = new Date(day.data.Ano, day.data.Mes - 1, day.data.Dia);

        if ((date.getDay() === 0 || date.getDay() === 6 || day.data.Feriado) && hours > 0) {
            element.extra.innerHTML = "(extra)";
        }
        else if (hours > 8) {
            element.extra.innerHTML = "(" + (hours - 8).toString() + ":" + minutes + " extra)";
        }
        else {
            element.extra.innerHTML = "";
        }
    };

    $timesheet.exportar = function (link) {
        if (!link.defaultHref) {
            link.defaultHref = link.href;
        }

        let data = selectTimesheets.options[selectTimesheets.selectedIndex].data;
        data.ColaboradorID = inputUsuario.value;

        link.href = link.defaultHref + "?ColaboradorID=" + inputUsuario.value + "&ContatoID=" + data.ContatoID + "&PeriodoDe=" + data.PeriodoDe + "&PeriodoAte=" + data.PeriodoAte;
        //console.log("href ", link.href);
        return true;
    };

    var getTime = function (timeString) {
        let hour = parseFloat(timeString.substr(0, 2));
        let minute = parseFloat(timeString.substr(3, 2));

        return hour * 3600000 + minute * 60000;
    };
})(window.$timesheet = window.$timesheet || {});