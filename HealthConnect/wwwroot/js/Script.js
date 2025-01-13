// header Scroll script

const header = document.querySelector('#header');
const hamburgerLink = document.querySelector('.hamburger_container_link');

window.addEventListener('scroll', () => {
    const scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

    if (scrollPosition > 100) {
        header.classList.add('scrolled');
        //hamburgerLink.classList.add('hamburgerLink_scroll');
    } else {
        header.classList.remove('scrolled');
        //hamburgerLink.classList.remove('hamburgerLink_scroll');
    }
});



document.addEventListener('DOMContentLoaded', () => {
    const passwordInput = document.getElementById('password');
    const toggleIcon = document.querySelector('.fa-eye');

    toggleIcon.addEventListener('click', () => {
        const isPasswordVisible = passwordInput.type === 'text';
        passwordInput.type = isPasswordVisible ? 'password' : 'text';
        toggleIcon.classList.toggle('fa-eye-slash', !isPasswordVisible);
        toggleIcon.classList.toggle('fa-eye', isPasswordVisible);
    });
});
