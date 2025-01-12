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
