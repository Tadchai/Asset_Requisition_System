function addClassification()
{
  const classificationDiv = document.getElementById("classifications");
  const tempClassification = document.getElementById("tempClassification");
  const tempClassClone = tempClassification.content.cloneNode(true);
  const instanceDiv = tempClassClone.querySelector(".instances")

  tempClassClone.querySelector(".addInstance").addEventListener("click", function ()
  {
    const tempInstance = document.getElementById("tempInstance");
    const tempInClone = tempInstance.content.cloneNode(true);

    tempInClone.querySelector(".deleteInstance").addEventListener("click", function ()
    {
      this.parentNode.remove();
    })

    instanceDiv.appendChild(tempInClone);
  })

  tempClassClone.querySelector(".deleteClassification").addEventListener("click", function ()
  {
    this.parentNode.remove();
  })

  classificationDiv.appendChild(tempClassClone);
}

async function save()
{
  let result = [];

  const classContainer = document.querySelectorAll(".classification-container");
  classContainer.forEach(x =>
  {
    const instanceResult = [];
    const inContainer = x.querySelectorAll(".instance-container")
    inContainer.forEach(y =>
    {
      instanceResult.push({ assetId: y.querySelector(".inputInstance").value })
    })

    result.push({ name: x.querySelector(".inputClassification").value, itemInstances: instanceResult });

  })

  const categoryName = document.querySelector(".inputCategory").value;

  const payload = {
    name: categoryName,
    itemClassifications: result,
  };

  try
  {
    const response = await fetch("http://localhost:5143/Item/Create", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (!response.ok)
    {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    const responseData = await response.json();

    if (responseData.statusCode === 201)
    {
      const idCategory = responseData.id;
      alert(responseData.message);
      window.location.href = `/Frontend/itemdetail.html?id=${idCategory}`;
    } else
    {
      alert(responseData.message || "Something went wrong");
    }
  } catch (error)
  {
    console.error("Error:", error);
    alert("An error occurred while creating the item.");
  }
}
