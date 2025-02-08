function redirectToHomepage() {
    window.location.href = "/";
}
function redirectToExplorepage() {
    window.location.href = "/User/Explore_doctors";
}

// Loder Script
document.addEventListener('DOMContentLoaded', () => {
    const loader = document.getElementById('loder');
    loader.classList.remove('hidden');
});

window.addEventListener('load', () => {
    const loader = document.getElementById('loder');
    loader.classList.add('hidden'); 
});

window.addEventListener('focus', () => {
    const loader = document.getElementById('loder');
    loader.classList.add('hidden');
});



// header Scroll script
const header = document.querySelector('#header');
const hamburgerLink = document.querySelector('.hamburger_container_link');

window.addEventListener('scroll', () => {
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



// Function to fetch currency symbol using the country code and currency code
async function fetchCurrencySymbol() {
    try {
        // Fetch country data from the API
        const response = await fetch("https://restcountries.com/v3.1/all");
        const countries = await response.json();

        // Get all doctor divs
        const doctorDivs = document.querySelectorAll('.all_doctor_single_div');

        doctorDivs.forEach(doctorDiv => {
            // Get the country and currency code from data attributes
            const countryCode = doctorDiv.querySelector('.all_doctor_single_left').getAttribute('data-country');
            const currencyCode = doctorDiv.querySelector('.all_doctor_single_left').getAttribute('data-currency');

            // Find the country by its country code
            const country = countries.find(c => c.cca2 === countryCode);

            if (country && country.currencies) {
                const currency = country.currencies[currencyCode];
                if (currency && currency.symbol) {
                    // Update the fee elements with the correct currency symbol
                    const feeOnSiteElement = document.getElementById(`fee-on-site-${countryCode}`);
                    const feeVideoCallElement = document.getElementById(`fee-video-call-${countryCode}`);
                    const feeHospitalOrClinicElement = document.getElementById(`fee-hospital-or-clinic-${countryCode}`);

                    if (feeOnSiteElement) {
                        feeOnSiteElement.innerHTML = `<strong>${currency.symbol}${feeOnSiteElement.innerText.replace(/[^\d.-]/g, '')}</strong> On site consultation`;
                    }

                    if (feeVideoCallElement) {
                        feeVideoCallElement.innerHTML = `<strong>${currency.symbol}${feeVideoCallElement.innerText.replace(/[^\d.-]/g, '')}</strong> Video consultation`;
                    }

                    if (feeHospitalOrClinicElement) {
                        feeHospitalOrClinicElement.innerHTML = `${currency.symbol}${feeHospitalOrClinicElement.innerText.replace(/[^\d.-]/g, '')}`;
                    }
                }
            }
        });
    } catch (error) {
        console.error("Error fetching currency symbol:", error);
    }
}

// Call the function on page load
window.onload = fetchCurrencySymbol;





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
    var doctorStep = document.getElementById("popup");
    var closePopup = document.getElementById("closePopup");
    var consultancy = document.querySelectorAll(".video-call, .Book-Onsite-Appointment");

    if (!doctorStep) return; // Exit if popup does not exist

    // Open popup and disable scrolling
    consultancy.forEach(function (button) {
        button.addEventListener("click", function () {
            doctorStep.style.display = "flex";
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

