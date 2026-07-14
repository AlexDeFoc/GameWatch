1. Make Lang Manager (before continuying, add sqlite_orm to project and maybe a class to contain it!)
2. write guide on setup/use terminal, CLion

# Upgrades to be made with C++26

* Instead of using const ptr to returning target, and having it act like a optional ref, we can finally natively return
std::optinal<T&>