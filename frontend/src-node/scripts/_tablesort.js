function sortTable(n) {
  var table,
    rows,
    switching,
    i,
    x,
    y,
    shouldSwitch,
    direction,
    switchcount = 0;
  table = $("table.sortable");
  switching = true;
  direction = "a";

  while (switching) {
    switching = false;
    rows = table.find("tr");

    for (i = 1; i < rows.length - 1; i++) {
      shouldSwitch = false;

      x = rows[i].getElementsByTagName("TD")[n];
      y = rows[i + 1].getElementsByTagName("TD")[n];

      if (direction === "a") {
        if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
          shouldSwitch = true;
          break;
        }
      } else if (direction === "d") {
        if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
          shouldSwitch = true;
          break;
        }
      }
    }

    if (shouldSwitch) {
      rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
      switching = true;

      switchcount++;
    } else {
      if (switchcount === 0 && direction === "a") {
        direction = "d";
        switching = true;
      }
    }
  }
  return direction;
}

$(function () {
  $("table.sortable thead th").on("click", function () {
    var headingCells = $("table.sortable thead tr th");

    headingCells
      .removeClass("sorted")
      .removeClass("dir-a")
      .removeClass("dir-d");

    var index = headingCells.index($(this));

    var direction = sortTable(index);

    $(this).addClass("sorted dir-" + direction);
  });
});
