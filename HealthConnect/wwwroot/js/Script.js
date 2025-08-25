function redirectToHomepage() {
    window.location.href = "/";
}

function redirectToPharmacyPage() {
    window.location.href = "/Pharmacy";
}

function redirectToExplorepage() {
    window.location.href = "/User/Explore_doctors";
}

// Loder Script
document.addEventListener('DOMContentLoaded', () => {
    const loader = document.getElementById('loder');
    if (loader) {

        loader.classList.remove('hidden');
    }
});

window.addEventListener('load', () => {
    const loader = document.getElementById('loder');
    if (loader) {

        loader.classList.add('hidden');
    }
});

window.addEventListener('focus', () => {
    const loader = document.getElementById('loder');
    if (loader) {

        loader.classList.add('hidden');
    }
});



// header Scroll script
const header = document.querySelector('#header');
const hamburgerLink = document.querySelector('.hamburger_container_link');
if (header) {

window.addEventListener('scroll', () => {
    const scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

    if (scrollPosition > 70) {
        header.classList.add('scrolled');
    } else {
        header.classList.remove('scrolled');
    }
});
}



document.addEventListener('DOMContentLoaded', () => {
    const passwordInput = document.getElementById('password');
    const toggleIcon = document.querySelector('.fa-eye');

    if (passwordInput && toggleIcon) {
        toggleIcon.addEventListener('click', () => {
            const isPasswordVisible = passwordInput.type === 'text';
            passwordInput.type = isPasswordVisible ? 'password' : 'text';
            toggleIcon.classList.toggle('fa-eye-slash', !isPasswordVisible);
            toggleIcon.classList.toggle('fa-eye', isPasswordVisible);
        });
    }
});


//document.addEventListener('DOMContentLoaded', () => {
//    document.querySelectorAll('a[href=""]'). forEach(anchor => {
//        anchor.addEventListener("click", function (event) {
//            event.preventDefault();
//        });
//    })
//});




document.addEventListener("DOMContentLoaded", function () {
    var doctorStep = document.getElementById("popup");
    var popup2 = document.getElementById("popup2");
    var closePopup = document.getElementById("closePopup");
    var closePopup2 = document.getElementById("closePopup2");
    var consultancy = document.querySelectorAll(".video-call, .Book-Onsite-Appointment, .reschedule_appointmant, .edit_profilepic, .give_feedback");
    var cancleAppointment = document.querySelectorAll(".cancle_Appointment, .remove_profilepic, .give_report");

    if (!doctorStep) return; // Exit if popup does not exist

    // Open popup and disable scrolling
    consultancy.forEach(function (button) {
        button.addEventListener("click", function () {
            doctorStep.style.display = "flex";
            document.body.classList.add("no-scroll"); // Disable scrolling
        });
    });
    // Open popup and disable scrolling
    cancleAppointment.forEach(function (button) {
        button.addEventListener("click", function () {
            popup2.style.display = "flex";
            document.body.classList.add("no-scroll"); // Disable scrolling
        });
    });

    // Close popup and enable scrolling
    if (closePopup) {
        closePopup.addEventListener("click", function () {
            doctorStep.style.display = "none";
            document.body.classList.remove("no-scroll"); // Enable scrolling
        });
    }

    if (closePopup2) {
        closePopup2.addEventListener("click", function () {
            popup2.style.display = "none";
            document.body.classList.remove("no-scroll"); // Enable scrolling
        });
    }
});


document.addEventListener("DOMContentLoaded", function () {
    function animateCountUp(element, start, end, duration) {
        let startTime = null;
        function step(currentTime) {
            if (!startTime) startTime = currentTime;
            let progress = Math.min((currentTime - startTime) / duration, 1);
            element.innerText = Math.floor(progress * (end - start) + start);
            if (progress < 1) {
                requestAnimationFrame(step);
            }
        }
        requestAnimationFrame(step);
    }

    // Get all elements with a data-count attribute
    document.querySelectorAll("[data-count]").forEach(countElement => {
        let targetCount = parseInt(countElement.getAttribute("data-count"));

        let observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCountUp(countElement, 0, targetCount, 500); // 0.5 sec animation
                    observer.disconnect(); // Stop observing after first trigger
                }
            });
        }, { threshold: 0.5 }); // Trigger when 50% of the element is visible

        observer.observe(countElement);
    });
});


document.addEventListener("DOMContentLoaded", function () {
    let detailsLeft = document.querySelector(".why_choose_us_left");
    let detailsRight = document.querySelectorAll(".why_choose_us_detail");
    let welcomeTexts = document.querySelectorAll("#welcome_text1, #welcome_text2");
    let DownUpAnimation = document.querySelectorAll(".DownUpAnimation");

    let observer = new IntersectionObserver((entries, observer) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                entry.target.style.transitionDelay = index * 300 + "ms";
                entry.target.classList.add("visible");
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    let observer2 = new IntersectionObserver((entries, observer2) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                entry.target.style.transitionDelay = index * 300 + "ms";
                entry.target.classList.add("visible2");
                observer2.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    let observer3 = new IntersectionObserver((entries, observer3) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                entry.target.style.transitionDelay = index * 300 + "ms";
                entry.target.classList.add("visible3");
                observer3.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    if (detailsLeft) observer.observe(detailsLeft);
    detailsRight.forEach((detail) => observer.observe(detail));
    welcomeTexts.forEach((text) => observer2.observe(text));
    DownUpAnimation.forEach((text) => observer3.observe(text));
});




