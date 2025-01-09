// header Scroll script

const header = document.getElementById('header');

window.addEventListener('scroll', function () {
    const scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

    if (scrollPosition > 100) {
        header.classList.add('scrolled');
    } else {
        header.classList.remove('scrolled');
    }
});