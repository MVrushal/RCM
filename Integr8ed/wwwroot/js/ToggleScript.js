// isActive Toggle
function statusToggle(id, status, isDisabled = false) {
  
    //    var content = `<div class="custom-control custom-switch">
    //  <input type="checkbox" class="custom-control-input" id="ck${id}" {checked} {disabled} data-id="{id}">
    //  <label class="custom-control-label" for="ck${id}"></label>
    //</div>`;


    var content = `<label class="switch">
  <input type="checkbox" id="ck${id}" {checked} {disabled} data-id="{id}">
  <span class="slider round"></span>
  </label>`;




    content = content.replace("{id}", id);
    content = content.replace("{checked}", status === true ? "checked" : "");
    content = content.replace("{disabled}", isDisabled === true ? "disabled" : "");

    console.log(content);
    return content;
}