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
