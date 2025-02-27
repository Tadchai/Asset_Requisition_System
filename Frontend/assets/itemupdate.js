let result = [];

function updateInstanceNumbers(classIndex, startIndex)
{
  const classDiv = document.querySelectorAll(".classification-container");
  const instances = classDiv[classIndex].querySelectorAll(".instance-container");

  for (let i = startIndex; i < instances.length; i++)
  {
    instances[i].dataset.index = i;
    const labelInstance = instances[i].querySelector(".instance-label");
    labelInstance.innerHTML = `${classIndex + 1}.${i + 1} - Asset ID:`;
  }
}

function updateClassificationNumbers(startIndex)
{
  const classDivs = document.querySelectorAll(".classification-container");

  for (let i = startIndex; i < classDivs.length; i++)
  {
    classDivs[i].dataset.index = i;
    classDivs[i].querySelector(".classification-label").innerHTML = `${i + 1} - Item Classification:`;

    updateInstanceNumbers(i, 0);
  }
}

window.addEventListener("DOMContentLoaded", async function ()
{
  const urlParams = new URLSearchParams(window.location.search);
  const itemCategoryId = urlParams.get("id");

  const response = await fetch(`http://localhost:5143/Item/GetById?id=${itemCategoryId}`);
  const data = await response.json();
  result = data
  render(data);

  document.querySelector(".updateCategory").addEventListener("change", function ()
  {
    result.name = this.value
  });
  document.querySelectorAll(".inputClassification").forEach(input =>
  {
    input.addEventListener("change", function ()
    {
      const index = this.parentNode.dataset.index;
      result.itemClassifications[index].name = this.value;
    });
  });

});

function render(data)
{
  let x = 0;
  document.querySelector(".updateCategory").value = data.name;
  document.querySelector(".updateCategory").dataset.id = data.id;

  const classificationDiv = document.getElementById("updateclassifications");

  data.itemClassifications.forEach(classification =>
  {
    let y = 0;
    const tempClassification = document.getElementById("tempClassification");
    const tempClassClone = tempClassification.content.cloneNode(true);
    const instanceDiv = tempClassClone.querySelector(".instances");
    const classContainer = tempClassClone.querySelector(".classification-container");
    classContainer.dataset.index = x;

    const inputClassification = tempClassClone.querySelector(".inputClassification");
    inputClassification.value = classification.name;
    inputClassification.dataset.id = classification.id;

    classification.itemInstances.forEach(instance =>
    {
      const tempInstance = document.getElementById("tempInstance");
      const tempInClone = tempInstance.content.cloneNode(true);
      const instanceContainer = tempInClone.querySelector(".instance-container");
      instanceContainer.dataset.index = y;

      const inputInstance = tempInClone.querySelector(".inputInstance");
      inputInstance.value = instance.assetId;
      inputInstance.dataset.id = instance.id;

      inputInstance.addEventListener("change", function ()
      {
        const classIndex = classContainer.dataset.index;
        const insIndex = instanceContainer.dataset.index;
        result.itemClassifications[classIndex].itemInstances[insIndex].assetId = this.value;
      });

      tempInClone.querySelector(".deleteInstance").addEventListener("click", function ()
      {
        const classIndex = parseInt(classContainer.dataset.index);
        const deletedIndex = parseInt(instanceContainer.dataset.index);
        this.parentNode.remove();

        updateInstanceNumbers(classIndex, deletedIndex);
      });

      instanceDiv.appendChild(tempInClone);
      y++;
    });

    tempClassClone.querySelector(".addInstance").addEventListener("click", function ()
    {
      const tempInstance = document.getElementById("tempInstance");
      const tempInClone = tempInstance.content.cloneNode(true);
      const instanceContainer = tempInClone.querySelector(".instance-container");
      const y = instanceDiv.querySelectorAll(".instance-container").length;
      instanceContainer.dataset.index = y;

      const classIndex = parseInt(classContainer.dataset.index);
      tempInClone.querySelector(".instance-label").innerHTML = `${classIndex + 1}.${y + 1} - Asset ID:`;
      result.itemClassifications[classIndex].itemInstances.push({
        id: null,
        assetId: "",
      });

      const inputInstance = tempInClone.querySelector(".inputInstance");
      inputInstance.addEventListener("change", function ()
      {
        result.itemClassifications[classIndex].itemInstances[y].assetId = this.value;
      });

      tempInClone.querySelector(".deleteInstance").addEventListener("click", function ()
      {

        const deletedIndex = parseInt(instanceContainer.dataset.index);
        this.parentNode.remove();
        result.itemClassifications[classIndex].itemInstances.splice(deletedIndex, 1);
        updateInstanceNumbers(classIndex, deletedIndex);
      });

      instanceDiv.appendChild(tempInClone);
    });

    tempClassClone.querySelector(".deleteClassification").addEventListener("click", function ()
    {
      const deletedIndex = parseInt(classContainer.dataset.index);
      this.parentNode.remove();
      result.itemClassifications.splice(deletedIndex, 1);
      updateClassificationNumbers(deletedIndex);
    });

    classificationDiv.appendChild(tempClassClone);
    x++;
  });

  updateClassificationNumbers(0);
}

function addClassification()
{
  const classificationDiv = document.getElementById("updateclassifications");
  const tempClassification = document.getElementById("tempClassification");
  const tempClassClone = tempClassification.content.cloneNode(true);

  const classContainer = tempClassClone.querySelector(".classification-container");
  const classIndex = document.querySelectorAll(".classification-container").length;
  classContainer.dataset.index = classIndex;

  classContainer.querySelector(".classification-label").innerHTML = `${classIndex + 1} - Item Classification:`;
  result.itemClassifications.push({
    id: null,
    name: "",
    itemInstances: []
  });

  const instanceDiv = classContainer.querySelector(".instances");

  classContainer.querySelector(".addInstance").addEventListener("click", function ()
  {
    const tempInstance = document.getElementById("tempInstance");
    const tempInClone = tempInstance.content.cloneNode(true);

    const instanceContainer = tempInClone.querySelector(".instance-container");
    const insIndex = instanceDiv.querySelectorAll(".instance-container").length;
    instanceContainer.dataset.index = insIndex;

    instanceContainer.querySelector(".instance-label").innerHTML = `${classIndex + 1}.${insIndex + 1} - Asset ID:`;
    result.itemClassifications[classIndex].itemInstances.push({
      id: null,
      assetId: "",
    });

    const inputInstance = tempInClone.querySelector(".inputInstance");
    inputInstance.addEventListener("change", function ()
    {
      result.itemClassifications[classIndex].itemInstances[insIndex].assetId = this.value;
    });

    instanceContainer.querySelector(".deleteInstance").addEventListener("click", function ()
    {
      const deletedIndex = parseInt(instanceContainer.dataset.index);
      this.parentNode.remove();
      result.itemClassifications[classIndex].itemInstances.splice(deletedIndex, 1);
      updateInstanceNumbers(classIndex, deletedIndex);
    });

    instanceDiv.appendChild(instanceContainer);

  });

  classContainer.querySelector(".deleteClassification").addEventListener("click", function ()
  {
    const deletedIndex = parseInt(classContainer.dataset.index);
    this.parentNode.remove();
    result.itemClassifications.splice(deletedIndex, 1)
    updateClassificationNumbers(deletedIndex);
  });

  classificationDiv.appendChild(classContainer);

  document.querySelectorAll(".inputClassification").forEach(input =>
  {
    input.addEventListener("change", function ()
    {
      const index = this.parentNode.dataset.index;
      result.itemClassifications[index].name = this.value;
    });
  });
}

async function save()
{
  try
  {
    const response = await fetch("http://localhost:5143/Item/Update", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(result),
    });
    const responseData = await response.json();
    alert(responseData.message || "Update completed successfully!");
    window.location.href = `/Frontend/itemdetail.html?id=${result.id}`;
  } catch (error)
  {
    console.error("Error updating item detail:", error);
    alert("An error occurred while updating item details.");
  }
}