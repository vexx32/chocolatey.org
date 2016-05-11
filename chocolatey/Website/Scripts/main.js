var els = document.querySelectorAll('[data-animate]');

Array.from(els).forEach(animateEl);

function animateEl(el) {
  var phrases = el.dataset.animate.split(',');
  var index = 0;
  var position = 0;
  var currentString = ''
  var direction = 1;
  var animate = function(){
    position+= direction;
    if (!phrases[index]) {
      index = 0;
    } else if (position < -1){
      index++;
      direction = 1;
    } else if (phrases[index][position] !== undefined) {
      currentString = phrases[index].substr(0, position);
    // if we've arrived at the last position reverse the direction
    } else if (position > 0 && !phrases[index][position]) {
      currentString = phrases[index].substr(0, position);
      direction = -1;
      el.innerText = currentString;
      return setTimeout(animate, 2000);
    }
    el.innerText = currentString;
    setTimeout(animate, 100);
  }
  animate();
}

var els = document.querySelectorAll('[data-copy-to-clipboard]');

Array.from(els).forEach(copyToClipboard);

var tmpElement = document.createElement('input');
tmpElement.className = 'invisible-input';
document.body.appendChild(tmpElement);

function copyToClipboard(el) {
  el.addEventListener('click', function(event){
    var parent = event.target.parentElement;
    if (!parent.dataset['copy-to-clipboard']) {
      parent = parent.parentElement;
    }
    var item = parent.querySelector('[data-clipboard-content]')
    var text = item.innerText;

    tmpElement.value = text;
    tmpElement.select();
    document.execCommand('copy');
  });
}


var closeButtons = Array.from(document.querySelectorAll('[data-close-message]'));

closeButtons.forEach(function(item){
  item.addEventListener('click', function(e){
    var el = e.target;
    if(el.dataset.closeMessage === undefined) {
      el = el.parentElement;
    }
    el.parentElement.parentElement.className += ' message--collapsed';
  });
});


